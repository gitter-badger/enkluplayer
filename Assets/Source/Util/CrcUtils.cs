using System.Text;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Utilities.
    /// </summary>
    public static class CrcUtils
    {
        /// <summary>
        /// Used for computation.
        /// </summary>
        private const ushort POLY = 0xA001;
        private static readonly ushort[] _Crc16Table = new ushort[256];

        /// <summary>
        /// Computes a crc for a string.
        /// </summary>
        /// <param name="str">Input string.</param>
        /// <returns></returns>
        public static ushort Crc16(string str)
        {
            return Crc16(Encoding.UTF8.GetBytes(str));
        }

        /// <summary>
        /// Computes a CRC.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <returns></returns>
        public static ushort Crc16(byte[] bytes)
        {
            ushort crc = 0;
            for (int i = 0, len = bytes.Length; i < len; ++i)
            {
                var index = (byte)(crc ^ bytes[i]);
                crc = (ushort)((crc >> 8) ^ _Crc16Table[index]);
            }

            return crc;
        }

        /// <summary>
        /// CRC16 taken from: https://stackoverflow.com/a/22861111
        /// </summary>
        static CrcUtils()
        {
            // calculate table for crc16
            for (ushort i = 0; i < _Crc16Table.Length; ++i)
            {
                ushort value = 0;
                var temp = i;
                for (byte j = 0; j < 8; ++j)
                {
                    if (((value ^ temp) & 0x0001) != 0)
                    {
                        value = (ushort)((value >> 1) ^ POLY);
                    }
                    else
                    {
                        value >>= 1;
                    }
                    temp >>= 1;
                }
                _Crc16Table[i] = value;
            }
        }
    }
}
