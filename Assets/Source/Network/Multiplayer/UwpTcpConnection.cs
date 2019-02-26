#if NETFX_CORE
using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using CreateAR.Commons.Unity.Logging;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Implementation that wraps <c>DataWriter</c>.
    /// </summary>
    public class DataWriterNetworkStream : INetworkStream
    {
        /// <summary>
        /// The writer.
        /// </summary>
        private readonly DataWriter _writer;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public DataWriterNetworkStream(DataWriter writer)
        {
            _writer = writer;
        }

        /// <inheritdoc />
        public void Write(byte[] buffer, int offset, int len)
        {
            _writer.WriteBuffer(buffer.AsBuffer(), (uint) offset, (uint) len);
        }
    }

    /// <summary>
    /// Uwp specific implementation of <c>ITcpConnection</c>.
    /// </summary>
    public class UwpTcpConnection : ITcpConnection
    {
        /// <summary>
        /// Global Connection Timeout Setting.
        /// </summary>
        public static TimeSpan ConnectionTimeout { get; set; }

        /// <summary>
        /// Listener for socket events.
        /// </summary>
        private readonly ISocketListener _listener;

        /// <summary>
        /// Writes to socket.
        /// </summary>
        private readonly ISocketMessageWriter _writer;

        /// <summary>
        /// Controls read thread
        /// </summary>
        private readonly AtomicBool _isReading = new AtomicBool(false);

        /// <summary>
        /// Controls initialization.
        /// </summary>
        private readonly AtomicBool _initialized = new AtomicBool(false);

        /// <summary>
        /// Underlying tcp socket.
        /// </summary>
        private StreamSocket _socket;

        /// <summary>
        /// Writes data to a stream.
        /// </summary>
        private DataWriter _dataWriter;

        /// <summary>
        /// Wraps DataWriter for the socket writer interface.
        /// </summary>
        private DataWriterNetworkStream _writerStream;

        /// <summary>
        /// Buffer we copy read data into.
        /// </summary>
        private byte[] _readBuffer = new byte[1024];

        /// <inheritdoc />
        public bool IsConnected { get; set; }

        /// <inheritdoc />
        public Action<bool> OnConnectionClosed { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public UwpTcpConnection(ISocketListener listener, ISocketMessageWriter writer)
        {
            ConnectionTimeout = TimeSpan.FromSeconds(3f);

            _listener = listener;
            _writer = writer;
        }

        /// <inheritdoc />
        public bool Connect(string host, int port)
        {
            if (!_initialized.CompareAndSet(false, true))
            {
                Log.Warning(this, "Tried to Connect using existing connection. Call Close() first.");
                return false;
            }

            HostName hostName;
            try
            {
                hostName = new HostName(host);
            }
            catch (Exception ex)
            {
                Log.Warning(this, $"Invalid hostname: {0}.", ex);

                SilentClose();
                return false;
            }
            
            _socket = new StreamSocket();
            _socket.Control.KeepAlive = true;
            _socket.Control.NoDelay = true;

            try
            {
                ConnectWith(_socket, hostName, port).Wait(ConnectionTimeout);
            }
            catch (Exception exception)
            {
                Log.Warning(this, "Connection failed: {0}", exception);
                SilentClose();

                return false;
            }
            
            // start socket reader thread
            ThreadHelper.SyncStart(ReadSocket);

            return true;
        }
        
        /// <inheritdoc />
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

        /// <inheritdoc />
        public void Close()
        {
            Close(true, false);
        }

        /// <summary>
        /// Closes the connection without dispatching.
        /// </summary>
        private void SilentClose()
        {
            Close(false, false);
        }

        /// <summary>
        /// Connects asynchronously.
        /// </summary>
        /// <param name="socket">The socket to connect with.</param>
        /// <param name="hostName">The host.</param>
        /// <param name="port">The port.</param>
        /// <returns></returns>
        private async Task ConnectWith(StreamSocket socket, HostName hostName, int port)
        {
            await socket.ConnectAsync(hostName, port.ToString());

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
                    var buffer = reader.ReadBuffer(len);

                    // copy into _readBuffer
                    if (_readBuffer.Length < buffer.Capacity)
                    {
                        _readBuffer = new byte[buffer.Capacity];
                    }
                    buffer.CopyTo(_readBuffer);
                    
                    // send to listener
                    _listener.HandleSocketMessage(new ArraySegment<byte>(_readBuffer, 0, _readBuffer.Length));
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