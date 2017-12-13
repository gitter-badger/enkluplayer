namespace CreateAR.SpirePlayer.Assets
{
    /// <summary>
    /// Interface for a hashing algorithm. This is not meant to provide a
    /// cryptographically secure hash.
    /// </summary>
    public interface IHashProvider
    {
        /// <summary>
        /// Provides a hash for bytes.
        /// </summary>
        /// <param name="bytes">Bytes to pass through hashing function.</param>
        /// <returns></returns>
        byte[] Hash(byte[] bytes);
    }
}