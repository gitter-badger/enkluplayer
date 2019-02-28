﻿#if !NETFX_CORE
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using CreateAR.Commons.Unity.Logging;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Connects to an endpoint over TCP.
    /// </summary>
    public class TcpConnection : ITcpConnection
    {
        /// <summary>
        /// Global Connection Timeout Setting.
        /// </summary>
        public static TimeSpan ConnectionTimeout { get; set; }

        private TcpClient _client;
        private readonly ISocketMessageReader _messageReader;
        private readonly ISocketMessageWriter _messageWriter;

        private readonly AtomicBool _isReading = new AtomicBool(false);
        private readonly AtomicBool _initialized = new AtomicBool(false);

        private readonly byte[] _readBuffer;
        private readonly MemoryStream _readStream;
        private readonly NetworkStreamWrapper _writeStream = new NetworkStreamWrapper();

        /// <summary>
        /// Whether or not the <see cref="TcpConnection"/> is connected to an endpoint.
        /// </summary>
        public bool IsConnected
        {
            get
            {
                return null != _client && _client.Connected;
            }
        }

        /// <summary>
        /// Fired when the connection is closed. The <see cref="bool"/> flag is set to
        /// <c>true</c> when the connection has been dropped from the server.
        /// </summary>
        public Action<bool> OnConnectionClosed { get; set; }

        public TcpClient TemporaryDebugging
        {
            get { return _client; }
        }

        /// <summary>
        /// Creates a new <see cref="TcpConnection"/> instance 
        /// </summary>
        /// <param name="messageReader"></param>
        /// <param name="messageWriter"></param>
        public TcpConnection(ISocketMessageReader messageReader, ISocketMessageWriter messageWriter)
        {
            ConnectionTimeout = TimeSpan.FromSeconds(3.0);

            _messageReader = messageReader;
            _messageWriter = messageWriter;

            _readBuffer = new byte[1024];
            _readStream = new MemoryStream();
        }

        /// <summary>
        /// Synchronously connects the the target host and port.
        /// </summary>
        public bool Connect(string host, int port)
        {
            if (!_initialized.CompareAndSet(false, true))
            {
                Log.Warning(this, "Tried to Connect using existing connection. Call Close() first.");
                return false;
            }

            // Try and Parse an IP Address from the provided "host"
            IPAddress ipAddress;
            if (!IPAddress.TryParse(host, out ipAddress))
            {
                Log.Warning(this, "Could not parse ipAddress.");
                SilentClose();

                return false;
            }


            // Use IP Address' Family
            _client = NewTcpClient(ipAddress.AddressFamily);

            // Connect directly using IPAddress and port
            try
            {
                ConnectWith(_client, ipAddress, port);
            }
            catch (Exception e)
            {
                Log.Warning(this, "Connection failed: {0}", e);
                SilentClose();

                return false;
            }

            // Valid TcpClient at this point, so check for connection
            if (!_client.Connected)
            {
                SilentClose();
                return false;
            }

            // Reset Message Reader and Start Read Thread
            _messageReader.Reset();

            // Starts the thread and waits for the thread to actually start before returning
            ThreadHelper.SyncStart(ReadSocket);

            return true;
        }

        /// <summary>
        /// Uses async connect with a synchronous Wait() up to <see cref="ConnectionTimeout"/>. This does not guarantee
        /// connection success, but ensures that we wait for no longer than the provided timeout. 
        /// </summary>
        private void ConnectWith(TcpClient client, IPAddress address, int port)
        {
            var asyncResult = client.BeginConnect(address, port, null, null);
            var success = asyncResult.AsyncWaitHandle.WaitOne(ConnectionTimeout);
            if (!success)
            {
                throw new Exception(string.Format(
                    "Failed to connect within {0} seconds.",
                    ConnectionTimeout.TotalSeconds));
            }

            client.EndConnect(asyncResult);

            if (!client.Connected)
            {
                throw new Exception("EndConnect completed but client is not connected.");
            }
        }
        
        /// <summary>
        /// Uses the <see cref="ISocketMessageWriter"/> to write data to the outbound TCP buffer.
        /// </summary>
        /// <param name="message">The binary payload to write to the outbound buffer.</param>
        /// <returns><c>true</c> if the write succeeded. Otherwise, <c>false</c></returns>
        public bool Send(byte[] message, int offset, int len)
        {
            try
            {
                var stream = _writeStream.Stream = _client.GetStream();
                if (!stream.CanWrite)
                {
                    Log.Warning(this, "Failed to write to outbound TCP Buffer: Stream is read-only.");
                    Close(true, false);
                    return false;
                }
                
                _messageWriter.Write(_writeStream, message, offset, len);
            }
            catch (Exception exception)
            {
                Log.Warning(this, "Failed to write to outbound TCP Buffer: {0}", exception);
                Close(true, false);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Performs a blocking read from the network stream. This
        /// </summary>
        private void ReadSocket()
        {
            NetworkStream stream;
            try
            {
                stream = _client.GetStream();
            }
            catch (Exception exception)
            {
                Log.Warning(this, "Could not get stream from TcpClient: {0}.", exception);
                Close(true, false);
                return;
            }

            if (!stream.CanRead)
            {
                Log.Warning(this, "Failed to read from inbound TCP buffer: Stream is not readable.");
                Close(true, false);
                return;
            }

            if (!_isReading.CompareAndSet(false, true))
            {
                Log.Warning(this, "Read Thread is already reading.");
                Close(true, false);
                return;
            }

            try
            {
                while (_isReading.Get())
                {
                    long resetPosition = 0;

                    // Blocking Read: 0 return for disconnected from server
                    int bytesRead = stream.Read(_readBuffer, 0, _readBuffer.Length);
                    if (bytesRead <= 0)
                    {
                        // Dispatch a Closed Connection for Closed Stream
                        Close(true, true);
                        return;
                    }

                    // In the event that we still have data remaining on the previous dataHandler call,
                    // prepend the remaining data on the new buffer. Otherwise, clear out the buffer and 
                    // start again.
                    if (_readStream.Position != _readStream.Length)
                    {
                        resetPosition = _readStream.Position;
                        _readStream.Position = _readStream.Length;
                    }
                    else
                    {

                        _readStream.Clear();
                    }

                    _readStream.Write(_readBuffer, 0, bytesRead);
                    _readStream.Position = resetPosition;

                    // Message Reader will advance the Position of the stream as it reads. If it does not
                    // consume all of the data, we will append the next read.
                    _messageReader.DataRead(_readStream);
                }
            }
            catch (ThreadAbortException e)
            {
                // We had to force abort on the thread, we do _not_ try and close again
            }
            catch (Exception exception)
            {
                //Log.Warning(this, "Disconnected: {0}", exception);

                // Since the read thread doesn't start until after we have connected, we can generally expect
                // an exception thrown here to be the result of a socket disconnection. We'll want to dispatch
                Close(true, false);
            }
            finally
            {
                _isReading.Set(false);

                //_messageReader.Reset();
                //_readStream.Clear();
            }
        }

        /// <summary>
        /// Closes the TCP connection and stops the socket reading thread.
        /// </summary>
        public void Close()
        {
            Close(true, false);
        }

        /// <summary>
        /// Closes the Tcp connection without dispatch.
        /// </summary>
        private void SilentClose()
        {
            Close(false, false);
        }

        /// <summary>
        /// Internal close which can optionally dispatch.
        /// </summary>
        private void Close(bool dispatch, bool closedStream)
        {
            // Prevent re-entry of Close()
            if (!_initialized.CompareAndSet(true, false))
            {
                return;
            }

            // Flip Read Flag
            _isReading.Set(false);

            // Close Client Connection
            CloseClient(_client);

            if (!_readThread.Join(TimeSpan.FromSeconds(0.5)))
            {
                _readThread.Abort();
                _readThread.Join();
            }

            _readThread = null;

            // Closing the connection will cause the blocking Read() to
            // throw (the stream closes), thus exiting the read thread
            _client = null;

            // If the client existed prior to closing and flagged for dispatch
            if (dispatch)
            {
                if (null != OnConnectionClosed)
                {
                    OnConnectionClosed(closedStream);
                }
            }
        }

        /// <summary>
        /// Closes a specific <see cref="TcpClient"/> used for the ipv6 connect attempts as well
        /// as cleanup of the internal tcp clients.
        /// </summary>
        private void CloseClient(TcpClient client)
        {
            try
            {
                if (null != client)
                {
                    client.Close();
                }
            }
            catch (Exception e)
            {
                Log.Info(this, "Closing Socket Exception: {0}", e);
            }
        }
        
        /// <summary>
        /// Creates a new <see cref="TcpClient"/> instance.
        /// </summary>
        private static TcpClient NewTcpClient(AddressFamily addressFamily)
        {
            return new TcpClient(addressFamily) { NoDelay = true };
        }
    }
}
#endif