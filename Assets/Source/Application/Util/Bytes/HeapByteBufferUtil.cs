namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Copyright (c) Microsoft. All rights reserved.
    /// Licensed under the MIT license. See LICENSE file in the project root for full license information.
    ///
    /// https://github.com/Azure/DotNetty/blob/2c0c10939e457a13b2299a5f8d7193d4c8206e95/src/DotNetty.Buffers/HeapByteBufferUtil.cs
    /// </summary>
    public static class HeapByteBufferUtil
    {
        internal static byte GetByte(byte[] memory, int index)
        {
            return memory[index];
        }

        internal static short GetShort(byte[] memory, int index)
        {
            return unchecked((short)(memory[index] << 8 | memory[index + 1]));
        }
        
        internal static int GetInt(byte[] memory, int index)
        {
            return unchecked(
                memory[index] << 24 |
                memory[index + 1] << 16 |
                memory[index + 2] << 8 |
                memory[index + 3]);
        }
        
        internal static long GetLong(byte[] memory, int index)
        {
            return unchecked(
                (long)memory[index] << 56 |
                (long)memory[index + 1] << 48 |
                (long)memory[index + 2] << 40 |
                (long)memory[index + 3] << 32 |
                (long)memory[index + 4] << 24 |
                (long)memory[index + 5] << 16 |
                (long)memory[index + 6] << 8 |
                memory[index + 7]);
        }
        
        internal static void SetShort(byte[] memory, int index, int value)
        {
            unchecked
            {
                memory[index] = (byte)((ushort)value >> 8);
                memory[index + 1] = (byte)value;
            }
        }
        
        internal static void SetInt(byte[] memory, int index, int value)
        {
            unchecked
            {
                uint unsignedValue = (uint)value;
                memory[index] = (byte)(unsignedValue >> 24);
                memory[index + 1] = (byte)(unsignedValue >> 16);
                memory[index + 2] = (byte)(unsignedValue >> 8);
                memory[index + 3] = (byte)unsignedValue;
            }
        }
        
        internal static void SetLong(byte[] memory, int index, long value)
        {
            unchecked
            {
                ulong unsignedValue = (ulong)value;
                memory[index] = (byte)(unsignedValue >> 56);
                memory[index + 1] = (byte)(unsignedValue >> 48);
                memory[index + 2] = (byte)(unsignedValue >> 40);
                memory[index + 3] = (byte)(unsignedValue >> 32);
                memory[index + 4] = (byte)(unsignedValue >> 24);
                memory[index + 5] = (byte)(unsignedValue >> 16);
                memory[index + 6] = (byte)(unsignedValue >> 8);
                memory[index + 7] = (byte)unsignedValue;
            }
        }
        
        public static unsafe int SingleToInt32Bits(float value)
        {
            return *(int*)(&value);
        }

        public static unsafe float Int32BitsToSingle(int value)
        {
            return *(float*)(&value);
        }
    }
}