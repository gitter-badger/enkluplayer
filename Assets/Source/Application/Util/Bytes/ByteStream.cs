using System;
using System.Diagnostics;
using System.Text;
using CreateAR.Commons.Unity.Logging;
using Enklu.Mycerializer;

namespace CreateAR.EnkluPlayer
{
    public class ByteStream : IByteStream
    {
        private readonly int _offset;
        private readonly ByteArrayHandle _handle;

        private int _readerIndex = 0;
        private int _writerIndex = 0;

        public ByteStream(byte[] bytes, int offset)
            : this(new ByteArrayHandle(bytes), offset)
        {
            //
        }

        public ByteStream(ByteArrayHandle handle)
            : this(handle, 0)
        {

        }

        public ByteStream(ByteArrayHandle handle, int offset)
        {
            _handle = handle;
            _offset = offset;
        }
        
        public int ReadInt()
        {
            Verbose("ReadInt()");

            var val = HeapByteBufferUtil.GetInt(_handle.Buffer, _offset + _readerIndex);

            _readerIndex += 4;

            return val;
        }

        public long ReadLong()
        {
            Verbose("ReadLong()");

            var val = HeapByteBufferUtil.GetLong(_handle.Buffer, _offset + _readerIndex);

            _readerIndex += 8;

            return val;
        }

        public short ReadShort()
        {
            Verbose("ReadShort()");

            var val = HeapByteBufferUtil.GetShort(_handle.Buffer, _offset + _readerIndex);

            _readerIndex += 2;

            return val;
        }

        public ushort ReadUnsignedShort()
        {
            Verbose("ReadUshort()");

            var val = HeapByteBufferUtil.GetShort(_handle.Buffer, _offset + _readerIndex);

            _readerIndex += 2;

            return (ushort) val;
        }

        public byte ReadByte()
        {
            Verbose("ReadByte()");

            var val = HeapByteBufferUtil.GetByte(_handle.Buffer, _offset + _readerIndex);

            _readerIndex += 1;

            return val;
        }

        public char ReadChar()
        {
            Verbose("ReadChar()");

            var val = HeapByteBufferUtil.GetShort(_handle.Buffer, _offset + _readerIndex);

            _readerIndex += 2;

            return (char) val;
        }

        public float ReadFloat()
        {
            Verbose("ReadFloat()");

            return HeapByteBufferUtil.Int32BitsToSingle(ReadInt());
        }

        public double ReadDouble()
        {
            Verbose("ReadDouble()");

            return BitConverter.Int64BitsToDouble(ReadLong());
        }

        public bool ReadBoolean()
        {
            Verbose("ReadBoolean()");

            return 0 != ReadByte();
        }

        public string ReadString(ushort len, Encoding encoding)
        {
            Verbose("ReadString()");

            var val = Encoding.UTF8.GetString(_handle.Buffer, _readerIndex + _offset, len);

            _readerIndex += len;

            return val;
        }

        public void WriteInt(int value)
        {
            const int numBytes = 4;
            EnsureSpace(numBytes);

            HeapByteBufferUtil.SetInt(_handle.Buffer, _writerIndex + _offset, value);

            _writerIndex += numBytes;
        }

        public void WriteLong(long value)
        {
            const int numBytes = 8;
            EnsureSpace(numBytes);

            HeapByteBufferUtil.SetLong(_handle.Buffer, _writerIndex + _offset, value);

            _writerIndex += numBytes;
        }

        public void WriteShort(short value)
        {
            const int numBytes = 2;
            EnsureSpace(numBytes);

            HeapByteBufferUtil.SetShort(_handle.Buffer, _writerIndex + _offset, value);

            _writerIndex += numBytes;
        }

        public void WriteByte(byte value)
        {
            const int numBytes = 1;
            EnsureSpace(numBytes);

            _handle.Buffer[_offset + _writerIndex++] = value;
        }

        public void WriteUnsignedShort(ushort value)
        {
            WriteShort((short) value);
        }

        public void WriteChar(char value)
        {
            WriteShort((short) value);
        }

        public void WriteFloat(float value)
        {
            WriteInt(HeapByteBufferUtil.SingleToInt32Bits(value));
        }

        public void WriteDouble(double value)
        {
            WriteLong(BitConverter.DoubleToInt64Bits(value));
        }

        public void WriteBoolean(bool value)
        {
            WriteByte(value ? (byte) 1 : (byte) 0);
        }

        public void WriteString(string str, Encoding encoding)
        {
            var val = Encoding.UTF8.GetBytes(str);
            var numBytes = val.Length;

            EnsureSpace(numBytes);

            Array.Copy(val, 0, _handle.Buffer, _writerIndex + _offset, numBytes);

            _writerIndex += numBytes;
        }

        public void SetIndex(int reader, int writer)
        {
            _readerIndex = reader;
            _writerIndex = writer;
        }

        public int WriterIndex
        {
            get { return _writerIndex; }
        }

        private void EnsureSpace(int numBytes)
        {
            while (_handle.Buffer.Length - _writerIndex < numBytes)
            {
                _handle.Grow();
            }
        }

        [Conditional("LOGGING_VERBOSE")]
        private void Verbose(string format, params object[] replacements)
        {
            Log.Info(this, format, replacements);
        }
    }
}