#if NETFX_CORE
using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using CreateAR.Commons.Unity.Logging;
using CreateAR.EnkluPlayer;

namespace CreateAR.EnkluPlayer
{
    public class DataWriterNetworkStream : INetworkStream
    {
        private readonly DataWriter _writer;

        public DataWriterNetworkStream(DataWriter writer)
        {
            _writer = writer;
        }

        public void Write(byte[] buffer, int offset, int len)
        {
            _writer.WriteBuffer(buffer.AsBuffer(), (uint) offset, (uint) len);
        }
    }

    public class UwpTcpConnection : ITcpConnection
    {
        /// <summary>
        /// Global Connection Timeout Setting.
        /// </summary>
        public static TimeSpan ConnectionTimeout { get; set; }

        private readonly ISocketListener _listener;
        private readonly ISocketMessageWriter _writer;

        private readonly AtomicBool _isReading = new AtomicBool(false);
        private readonly AtomicBool _initialized = new AtomicBool(false);

        private StreamSocket _socket;
        private DataWriter _dataWriter;
        private DataWriterNetworkStream _writerStream;

        public bool IsConnected { get; set; }

        public Action<bool> OnConnectionClosed { get; set; }

        public UwpTcpConnection(ISocketListener listener, ISocketMessageWriter writer)
        {
            _listener = listener;
            _writer = writer;
        }

        public bool Connect(string host, int port)
        {
            if (!_initialized.CompareAndSet(false, true))
            {
                Log.Warning(this, "Tried to Connect using existing connection. Call Close() first.");
                return false;
            }
            
            _socket = new StreamSocket();
            _socket.Control.KeepAlive = true;
            _socket.Control.NoDelay = true;

            try
            {
                ConnectWith(_socket, host, port).Wait(ConnectionTimeout);
            }
            catch (Exception exception)
            {
                Log.Warning(this, "Connection failed: {0}", exception);
                SilentClose();

                return false;
            }
            
            ThreadHelper.SyncStart(ReadSocket);

            return true;
        }
        
        public bool Send(byte[] message, int offset, int len)
        {
            try
            {
                _writer.Write(_writerStream, message, offset, len);

                return true;
            }
            catch (Exception exception)
            {
                Log.Error(this, $"Could not send message : {exception}");

                return false;
            }
        }

        /// <summary>
        /// Closes the TCP connection and stops the socket reading thread.
        /// </summary>
        public void Close()
        {
            Close(true, false);
        }

        private void SilentClose()
        {
            Close(false, false);
        }

        private async Task ConnectWith(StreamSocket socket, string host, int port)
        {
            await socket.ConnectAsync(new HostName(host), port.ToString());

            _dataWriter = new DataWriter(socket.OutputStream);
            _writerStream = new DataWriterNetworkStream(_dataWriter);
        }

        /// <summary>
        /// Performs a blocking read from the network stream. This
        /// </summary>
        private async void ReadSocket()
        {
            DataReader reader;
            try
            {
                reader = new DataReader(_socket.InputStream);
            }
            catch (Exception exception)
            {
                Log.Warning(this, "Could not create DataReader from InputStream: {0}.", exception);
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
                    // read length
                    await reader.LoadAsync(sizeof(ushort));
                    var len = reader.ReadUInt16();
                    
                    // read payload
                    await reader.LoadAsync(len);
                    // TODO: figure out a better way to do this
                    var buffer = reader.ReadBuffer(len).ToArray();
                    
                    // send to listener
                    _listener.HandleSocketMessage(new ArraySegment<byte>(buffer, 0, buffer.Length));
                }
            }
            catch (Exception exception)
            {
                Log.Warning(this, "Disconnected: {0}", exception);

                // Since the read thread doesn't start until after we have connected, we can generally expect
                // an exception thrown here to be the result of a socket disconnection. We'll want to dispatch
                Close(true, false);
            }
            finally
            {
                _isReading.Set(false);

                reader.Dispose();
            }
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

            // close socket (which will kill reader)
            if (null != _socket)
            {
                _socket.Dispose();
                _socket = null;
            }

            // close writer
            if (null != _dataWriter)
            {
                _dataWriter.Dispose();
                _dataWriter = null;
                _writerStream = null;
            }

            // If the client existed prior to closing and flagged for dispatch
            if (dispatch)
            {
                OnConnectionClosed?.Invoke(closedStream);
            }
        }
    }
}
#endif