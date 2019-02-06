using System;

namespace CreateAR.EnkluPlayer
{
    public static class LengthFieldHelper
    {
        /// <summary>
        /// Determines if a frame length size is of legal size.
        /// </summary>
        public static bool IsLegalLengthSize(int lengthSize)
        {
            return lengthSize == sizeof(byte)
                   || lengthSize == sizeof(short)
                   || lengthSize == sizeof(int)
                   || lengthSize == sizeof(long);
        }

        /// <summary>
        /// Converts a byte array containing variable length of binary to a long frame length representation.
        /// </summary>
        public static long GetFrameLength(byte[] lengthData, int lengthSize)
        {
            if (sizeof(byte) == lengthSize)
            {
                return lengthData[0];
            }

            if (sizeof(short) == lengthSize)
            {
                return BitConverter.ToInt16(lengthData, 0);
            }

            if (sizeof(int) == lengthSize)
            {
                return BitConverter.ToInt32(lengthData, 0);
            }

            return BitConverter.ToInt64(lengthData, 0);
        }

        public static void WriteLength(long length, byte[] lengthBuffer, int lengthSize)
        {
            for (var i = 0; i < lengthSize; ++i)
            {
                lengthBuffer[i] = (byte) (length >> (i * 8) & 0xFF);
            }
        }
    }
}