#if NETFX_CORE
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using CreateAR.Commons.Unity.Logging;
using Enklu.Mycerializer;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Naive implementation of <c>IMessageReader</c> that can read objects
    /// from a stream.
    /// </summary>
    public class UwpReflectionMessageReader : IMessageReader
    {
        /// <inheritdoc />
        public object Read(Type type, IByteStream buffer)
        {
            var isAllocated = buffer.ReadBoolean();
            if (!isAllocated)
            {
                return null;
            }

            var instance = Activator.CreateInstance(type);

            var fields = type
                .GetFields(BindingFlags.Instance | BindingFlags.Public)
                .OrderBy(f => f.Name);
            foreach (var field in fields)
            {
                var fieldType = field.FieldType;

                // detect cycles
                if (fieldType == type)
                {
                    continue;
                }

                field.SetValue(instance, ReadValue(fieldType, buffer));
            }

            return instance;
        }

        /// <summary>
        /// Reads a specific value from the buffer.
        /// </summary>
        /// <param name="fieldType">The type.</param>
        /// <param name="buffer">The buffer.</param>
        /// <returns></returns>
        private object ReadValue(Type fieldType, IByteStream buffer)
        {
            // handle primitives
            if (fieldType.GetTypeInfo().IsPrimitive)
            {
                if (typeof(int) == fieldType)
                {
                    return buffer.ReadInt();
                }

                if (typeof(long) == fieldType)
                {
                    return buffer.ReadLong();
                }

                if (typeof(short) == fieldType)
                {
                    return buffer.ReadShort();
                }

                if (typeof(ushort) == fieldType)
                {
                    return buffer.ReadUnsignedShort();
                }

                if (typeof(byte) == fieldType)
                {
                    return buffer.ReadByte();
                }

                if (typeof(char) == fieldType)
                {
                    return buffer.ReadChar();
                }

                if (typeof(float) == fieldType)
                {
                    return buffer.ReadFloat();
                }

                if (typeof(double) == fieldType)
                {
                    return buffer.ReadDouble();
                }

                if (typeof(bool) == fieldType)
                {
                    return buffer.ReadBoolean();
                }

                throw new Exception($"Cannot read primitive type '{fieldType.Name}'. Please switch to a supported primitive type.");
            }

            // strings
            if (typeof(string) == fieldType)
            {
                // length
                var len = buffer.ReadUnsignedShort();
                if (0 == len)
                {
                    return string.Empty;
                }

                return buffer.ReadString(len, Encoding.ASCII);
            }

            // DateTime
            if (typeof(DateTime) == fieldType)
            {
                var kind = (DateTimeKind)buffer.ReadInt();
                var unixTime = buffer.ReadDouble();
                var epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, kind);
                return epoch.AddMilliseconds(unixTime);
            }

            // handle arrays
            if (fieldType.IsArray)
            {
                var len = buffer.ReadUnsignedShort();
                var elType = fieldType.GetElementType();
                if (elType != null)
                {
                    var arr = Array.CreateInstance(elType, len);
                    for (var i = 0; i < len; i++)
                    {
                        arr.SetValue(ReadValue(elType, buffer), i);
                    }

                    return arr;
                }

                throw new Exception("Invalid type information: Array type with no element type.");
            }

            // handle lists
            if (fieldType.IsGenericType() && typeof(List<>) == fieldType.GetGenericTypeDefinition())
            {
                var len = buffer.ReadUnsignedShort();
                var elType = fieldType.GetGenericArguments()[0];
                var list = (IList)Activator.CreateInstance(fieldType);
                for (var i = 0; i < len; i++)
                {
                    list.Add(ReadValue(elType, buffer));
                }

                return list;
            }

            // handle dictionaries
            if (fieldType.IsGenericType() && typeof(IDictionary).IsAssignableFrom(fieldType))
            {
                var kType = fieldType.GetGenericArguments()[0];
                var vType = fieldType.GetGenericArguments()[1];

                var len = buffer.ReadUnsignedShort();
                var dict = (IDictionary)Activator.CreateInstance(fieldType);
                for (var i = 0; i < len; i++)
                {
                    var key = ReadValue(kType, buffer);
                    var value = ReadValue(vType, buffer);

                    dict[key] = value;
                }

                return dict;
            }

            // handle composites
            return Read(fieldType, buffer);
        }
    }

    /// <summary>
    /// Naive implementation of <c>IMessageWriter</c> that uses reflection to
    /// write objects to a stream.
    /// </summary>
    public class UwpReflectionMessageWriter : IMessageWriter
    {
        /// <inheritdoc />
        public void Write(object instance, IByteStream buffer)
        {
            // write a byte for allocation or not
            var allocated = null != instance;
            buffer.WriteBoolean(allocated);

            if (allocated)
            {
                var type = instance.GetType();
                var fields = type
                    .GetFields(BindingFlags.Instance | BindingFlags.Public)
                    .OrderBy(f => f.Name);
                foreach (var field in fields)
                {
                    var fieldType = field.FieldType;

                    // detect cycles
                    if (fieldType == type)
                    {
                        continue;
                    }

                    WriteValue(fieldType, field.GetValue(instance), buffer);
                }
            }
        }

        /// <summary>
        /// Writes a value to the stream.
        /// </summary>
        /// <param name="fieldType">The type.</param>
        /// <param name="value">The value.</param>
        /// <param name="buffer">The buffer to write to.</param>
        private void WriteValue(Type fieldType, object value, IByteStream buffer)
        {
            // handle primitives
            if (fieldType.GetTypeInfo().IsPrimitive)
            {
                if (typeof(int) == fieldType)
                {
                    buffer.WriteInt((int)value);
                    return;
                }

                if (typeof(long) == fieldType)
                {
                    buffer.WriteLong((long)value);
                    return;
                }

                if (typeof(short) == fieldType)
                {
                    buffer.WriteShort((short)value);
                    return;
                }

                if (typeof(ushort) == fieldType)
                {
                    buffer.WriteUnsignedShort((ushort)value);
                    return;
                }

                if (typeof(byte) == fieldType)
                {
                    buffer.WriteByte((byte)value);
                    return;
                }

                if (typeof(char) == fieldType)
                {
                    buffer.WriteChar((char)value);
                    return;
                }

                if (typeof(float) == fieldType)
                {
                    buffer.WriteFloat((float)value);
                    return;
                }

                if (typeof(double) == fieldType)
                {
                    buffer.WriteDouble((double)value);
                    return;
                }

                if (typeof(bool) == fieldType)
                {
                    buffer.WriteBoolean((bool)value);
                    return;
                }

                throw new Exception($"Cannot write primitive type '{fieldType.Name}'. Please switch to a supported primitive type.");
            }

            // strings
            if (typeof(string) == fieldType)
            {
                // length
                var str = (string)value;
                var len = 0;
                if (!string.IsNullOrEmpty(str))
                {
                    len = str.Length;
                }

                if (len > ushort.MaxValue)
                {
                    throw new Exception($"Cannot write string of length '{len}'. Max length is '{ushort.MaxValue}'. ");
                }

                buffer.WriteUnsignedShort((ushort)len);

                if (len > 0)
                {
                    buffer.WriteString(str, Encoding.ASCII);
                }

                return;
            }

            // DateTime
            if (typeof(DateTime) == fieldType)
            {
                var time = (DateTime)value;
                var epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, time.Kind);
                buffer.WriteInt((int)time.Kind);
                buffer.WriteDouble((time - epoch).TotalMilliseconds);
                return;
            }

            // handle arrays
            if (fieldType.IsArray)
            {
                var arr = (Array)value;
                var elType = fieldType.GetElementType();
                if (null == elType)
                {
                    throw new Exception("Invalid type information: Array type with no element type.");
                }

                var len = 0;
                if (null != arr)
                {
                    len = arr.Length;
                }

                if (len > ushort.MaxValue)
                {
                    throw new Exception($"Cannot write array of length '{len}'. Max len is '{ushort.MaxValue}'.");
                }

                buffer.WriteUnsignedShort((ushort)len);

                for (var i = 0; i < len; i++)
                {
                    var elValue = arr.GetValue(i);

                    WriteValue(elType, elValue, buffer);
                }

                return;
            }

            // handle lists
            if (fieldType.IsGenericType() && typeof(List<>) == fieldType.GetGenericTypeDefinition())
            {
                var list = (IList)value;
                var elType = fieldType.GetGenericArguments()[0];

                var len = 0;
                if (null != list)
                {
                    len = list.Count;
                }

                if (len > ushort.MaxValue)
                {
                    throw new Exception($"Cannot write List<{elType.Name}> of length '{len}'. Max len is '{ushort.MaxValue}'.");
                }

                buffer.WriteUnsignedShort((ushort)len);

                if (null != list)
                {
                    for (var i = 0; i < len; i++)
                    {
                        var elValue = list[i];

                        WriteValue(elType, elValue, buffer);
                    }
                }

                return;
            }

            // handle dictionaries
            if (fieldType.IsGenericType() && typeof(IDictionary).IsAssignableFrom(fieldType))
            {
                var dict = (IDictionary)value;
                var kType = fieldType.GetGenericArguments()[0];
                var vType = fieldType.GetGenericArguments()[1];

                var len = 0;
                if (null != dict)
                {
                    len = dict.Count;
                }

                if (len > ushort.MaxValue)
                {
                    throw new Exception($"Cannot write Dictionary<{kType.Name}, {vType.Name}> of length {len}. Max length is {ushort.MaxValue}.");
                }

                buffer.WriteUnsignedShort((ushort)len);

                if (null != dict)
                {
                    var keys = dict.Keys;
                    foreach (var key in keys)
                    {
                        var val = dict[key];

                        WriteValue(kType, key, buffer);
                        WriteValue(vType, val, buffer);
                    }
                }

                return;
            }

            if (typeof(object) == fieldType)
            {
                throw new Exception("Field of type 'object' is not supported. Polymorphism is not supported by this serializer: pick a type and stick to it.");
            }

            // handle composites
            Write(value, buffer);
        }
    }

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
            _writer.StoreAsync();
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
            ConnectionTimeout = TimeSpan.FromMilliseconds(3000);

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
                Log.Warning(this, "Connection to '{0}:{1}' failed: invalid hostname - {1}.",
                    host,
                    port,
                    ex);

                SilentClose();
                return false;
            }

            Verbose("Connect: Create socket.");
            
            _socket = new StreamSocket();
            _socket.Control.KeepAlive = true;
            _socket.Control.NoDelay = true;

            try
            {
                Verbose("Connect: Connect.");

                if (!ConnectWith(_socket, hostName, port))
                {
                    Log.Warning(this, "Connection to {0}:{1} failed: timed out.", host, port);
                    SilentClose();

                    return false;
                }
            }
            catch (Exception exception)
            {
                Log.Warning(this, "Connection to {0}:{1} failed: {2}", host, port, exception);
                SilentClose();

                return false;
            }

            Verbose("Connect: Start reader thread.");

            // start socket reader thread
            ThreadHelper.SyncStart(ReadSocket);

            Verbose("Connect: Complete.");
            
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
            Log.Info(this, "Closing Tcp connection.");

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
        private bool ConnectWith(StreamSocket socket, HostName hostName, int port)
        {
            if (!socket
                .ConnectAsync(hostName, port.ToString())
                .AsTask()
                .Wait(ConnectionTimeout))
            {
                return false;
            }
            
            _dataWriter = new DataWriter(socket.OutputStream);
            _dataWriter.ByteOrder = ByteOrder.BigEndian;
            _writerStream = new DataWriterNetworkStream(_dataWriter);
            
            Verbose("ConnectWith: Created writer.");

            IsConnected = true;

            return true;
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
                reader.ByteOrder = ByteOrder.BigEndian;
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

            Verbose("ReadSocket: Created reader.");

            try
            {
                while (_isReading.Get())
                {
                    Verbose("ReadSocket: Waiting.");

                    // read length
                    await reader.LoadAsync(sizeof(ushort));
                    var len = reader.ReadUInt16();

                    Verbose($"ReadSocket: Read length of {len} bytes.");

                    // read payload
                    await reader.LoadAsync(len);
                    var buffer = reader.ReadBuffer(len);

                    Verbose($"ReadSocket: Read payload of {buffer.Capacity} bytes.");

                    // copy into _readBuffer
                    if (_readBuffer.Length < buffer.Length)
                    {
                        _readBuffer = new byte[buffer.Length];
                    }
                    buffer.CopyTo(0, _readBuffer, 0, (int) buffer.Length);

                    Verbose("ReadSocket: Read message payload.");

                    // don't let the handler kill the reader
                    try
                    {
                        _listener.HandleSocketMessage(new ArraySegment<byte>(
                            _readBuffer,
                            0, (int) buffer.Length));
                    }
                    catch (Exception ex)
                    {
                        Log.Error(this, "Socket listener threw an exception: {0}.", ex);
                    }
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

        /// <summary>
        /// Verbose logging.
        /// </summary>
        [Conditional("LOGGING_VERBOSE")]
        private void Verbose(string format, params object[] replacements)
        {
            Log.Info(this, format, replacements);
        }
    }
}
#endif