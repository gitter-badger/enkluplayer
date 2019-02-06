using System;
using System.Text;
using Enklu.Mycerializer;

namespace CreateAR.EnkluPlayer
{
    public class ByteStream : IByteStream
    {
        private readonly byte[] _bytes;
        private readonly int _start;

        private int _readerIndex = 0;
        private int _writerIndex = 0;

        public ByteStream(byte[] bytes, int start)
        {
            _bytes = bytes;
            _start = start;
        }

        public int ReadInt()
        {
            var val = HeapByteBufferUtil.GetInt(_bytes, _start + _readerIndex);

            _readerIndex += 4;

            return val;
        }

        public long ReadLong()
        {
            var val = HeapByteBufferUtil.GetLong(_bytes, _start + _readerIndex);

            _readerIndex += 8;

            return val;
        }

        public short ReadShort()
        {
            var val = HeapByteBufferUtil.GetShort(_bytes, _start + _readerIndex);

            _readerIndex += 2;

            return val;
        }

        public ushort ReadUnsignedShort()
        {
            var val = HeapByteBufferUtil.GetShort(_bytes, _start + _readerIndex);

            _readerIndex += 2;

            return (ushort) val;
        }

        public byte ReadByte()
        {
            var val = HeapByteBufferUtil.GetByte(_bytes, _start + _readerIndex);

            _readerIndex += 1;

            return val;
        }

        public char ReadChar()
        {
            var val = HeapByteBufferUtil.GetShort(_bytes, _start + _readerIndex);

            _readerIndex += 2;

            return (char) val;
        }

        public float ReadFloat()
        {
            return HeapByteBufferUtil.Int32BitsToSingle(ReadInt());
        }

        public double ReadDouble()
        {
            return BitConverter.Int64BitsToDouble(ReadLong());
        }

        public bool ReadBoolean()
        {
            return 0 != ReadByte();
        }

        public string ReadString(ushort len, Encoding encoding)
        {
            var val = Encoding.UTF8.GetString(_bytes, _readerIndex + _start, len);

            _readerIndex += len;

            return val;
        }

        public void WriteInt(int value)
        {
            HeapByteBufferUtil.SetInt(_bytes, _writerIndex + _start, value);

            _writerIndex += 4;
        }

        public void WriteLong(long value)
        {
            HeapByteBufferUtil.SetLong(_bytes, _writerIndex + _start, value);

            _writerIndex += 8;
        }

        public void WriteShort(short value)
        {
            HeapByteBufferUtil.SetShort(_bytes, _writerIndex + _start, value);

            _writerIndex += 2;
        }

        public void WriteByte(byte value)
        {
            _bytes[_start + _writerIndex++] = value;
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

            Array.Copy(val, 0, _bytes, _writerIndex + _start, val.Length);

            _writerIndex += val.Length;
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
    }
}