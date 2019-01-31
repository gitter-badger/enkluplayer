namespace CreateAR.EnkluPlayer.Scripting
{
    /// <summary>
    /// Various utilities for checksums.
    /// </summary>
    [JsInterface("checksum")]
    public class ChecksumJsInterface
    {
        /// <summary>
        /// Calculates a CRC of a string payload.
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <returns></returns>
        public int crc16(string payload)
        {
            return CrcUtils.Crc16(payload);
        }
    }
}
