using System;
using System.Diagnostics;
using System.Text;
using CreateAR.Commons.Unity.Logging;
using Enklu.Mycerializer;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// <c>IByteStream</c> implementation for unity.
    /// </summary>
    public class ByteStream : IByteStream
    {
        /// <summary>
        /// The offset to apply to the every operation.
        /// </summary>
        private readonly int _offset;

        /// <summary>
        /// The managed byte array to use.
        /// </summary>
        private readonly ByteArrayHandle _handle;

        /// <summary>
        /// Index of the read head.
        /// </summary>
        private int _readerIndex = 0;

        /// <summary>
        /// Index of the write head.
        /// </summary>
        private int _writerIndex = 0;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="bytes">A byte array.</param>
        /// <param name="offset">The offset in the stream.</param>
        public ByteStream(byte[] bytes, int offset)
            : this(new ByteArrayHandle(bytes), offset)
        {
            //
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="handle">A handle to a byte array.</param>
        public ByteStream(ByteArrayHandle handle)
            : this(handle, 0)
        {

        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="handle">A handle to a byte array.</param>
        /// <param name="offset">The offset in the byte array.</param>
        public ByteStream(ByteArrayHandle handle, int offset)
        {
            _handle = handle;
            _offset = offset;
        }
        
        /// <inheritdoc />
        public int ReadInt()
        {
            Verbose("ReadInt()");

            var val = HeapByteBufferUtil.GetInt(_handle.Buffer, _offset + _readerIndex);

            _readerIndex += 4;

            return val;
        }

        /// <inheritdoc />
        public long ReadLong()
        {
            Verbose("ReadLong()");

            var val = HeapByteBufferUtil.GetLong(_handle.Buffer, _offset + _readerIndex);

            _readerIndex += 8;

            return val;
        }

        /// <inheritdoc />
        public short ReadShort()
        {
            Verbose("ReadShort()");

            var val = HeapByteBufferUtil.GetShort(_handle.Buffer, _offset + _readerIndex);

            _readerIndex += 2;

            return val;
        }

        /// <inheritdoc />
        public ushort ReadUnsignedShort()
        {
            Verbose("ReadUshort()");

            var val = HeapByteBufferUtil.GetShort(_handle.Buffer, _offset + _readerIndex);

            _readerIndex += 2;

            return (ushort) val;
        }

        /// <inheritdoc />
        public byte ReadByte()
        {
            Verbose("ReadByte()");

            var val = HeapByteBufferUtil.GetByte(_handle.Buffer, _offset + _readerIndex);

            _readerIndex += 1;

            return val;
        }

        /// <inheritdoc />
        public char ReadChar()
        {
            Verbose("ReadChar()");

            var val = HeapByteBufferUtil.GetShort(_handle.Buffer, _offset + _readerIndex);

            _readerIndex += 2;

            return (char) val;
        }

        /// <inheritdoc />
        public float ReadFloat()
        {
            Verbose("ReadFloat()");

            return HeapByteBufferUtil.Int32BitsToSingle(ReadInt());
        }

        /// <inheritdoc />
        public double ReadDouble()
        {
            Verbose("ReadDouble()");

            return BitConverter.Int64BitsToDouble(ReadLong());
        }

        /// <inheritdoc />
        public bool ReadBoolean()
        {
            Verbose("ReadBoolean()");

            return 0 != ReadByte();
        }

        /// <inheritdoc />
        public string ReadString(ushort len, Encoding encoding)
        {
            Verbose("ReadString()");

            var val = Encoding.UTF8.GetString(_handle.Buffer, _readerIndex + _offset, len);

            _readerIndex += len;

            return val;
        }

        /// <inheritdoc />
        public void WriteInt(int value)
        {
            const int numBytes = 4;
            EnsureSpace(numBytes);

            HeapByteBufferUtil.SetInt(_handle.Buffer, _writerIndex + _offset, value);

            _writerIndex += numBytes;
        }

        /// <inheritdoc />
        public void WriteLong(long value)
        {
            const int numBytes = 8;
            EnsureSpace(numBytes);

            HeapByteBufferUtil.SetLong(_handle.Buffer, _writerIndex + _offset, value);

            _writerIndex += numBytes;
        }

        /// <inheritdoc />
        public void WriteShort(short value)
        {
            const int numBytes = 2;
            EnsureSpace(numBytes);

            HeapByteBufferUtil.SetShort(_handle.Buffer, _writerIndex + _offset, value);

            _writerIndex += numBytes;
        }

        /// <inheritdoc />
        public void WriteByte(byte value)
        {
            const int numBytes = 1;
            EnsureSpace(numBytes);

            _handle.Buffer[_offset + _writerIndex++] = value;
        }

        /// <inheritdoc />
        public void WriteUnsignedShort(ushort value)
        {
            WriteShort((short) value);
        }

        /// <inheritdoc />
        public void WriteChar(char value)
        {
            WriteShort((short) value);
        }

        /// <inheritdoc />
        public void WriteFloat(float value)
        {
            WriteInt(HeapByteBufferUtil.SingleToInt32Bits(value));
        }

        /// <inheritdoc />
        public void WriteDouble(double value)
        {
            WriteLong(BitConverter.DoubleToInt64Bits(value));
        }

        /// <inheritdoc />
        public void WriteBoolean(bool value)
        {
            WriteByte(value ? (byte) 1 : (byte) 0);
        }

        /// <inheritdoc />
        public void WriteString(string str, Encoding encoding)
        {
            var val = Encoding.UTF8.GetBytes(str);
            var numBytes = val.Length;

            EnsureSpace(numBytes);

            Array.Copy(val, 0, _handle.Buffer, _writerIndex + _offset, numBytes);

            _writerIndex += numBytes;
        }

        /// <inheritdoc />
        public void SetIndex(int reader, int writer)
        {
            _readerIndex = reader;
            _writerIndex = writer;
        }

        /// <inheritdoc />
        public int WriterIndex
        {
            get { return _writerIndex; }
        }

        /// <summary>
        /// ensures there is enough space to write.
        /// </summary>
        /// <param name="numBytes"></param>
        private void EnsureSpace(int numBytes)
        {
            while (_handle.Buffer.Length - _writerIndex < numBytes)
            {
                _handle.Grow();
            }
        }

        /// <summary>
        /// Logging function for debugging that is compiled out, in general.
        /// </summary>
        [Conditional("LOGGING_VERBOSE")]
        private void Verbose(string format, params object[] replacements)
        {
            Log.Info(this, format, replacements);
        }
    }
}