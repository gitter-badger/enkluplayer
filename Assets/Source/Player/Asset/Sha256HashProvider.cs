#if UNITY_EDITOR || !UNITY_WSA
using System.Security.Cryptography;

namespace CreateAR.EnkluPlayer.Assets
{
    /// <summary>
    /// SHA256 implementation.
    /// </summary>
    public class Sha256HashProvider : IHashProvider
    {
        /// <summary>
        /// SHA implementation.
        /// </summary>
        private readonly SHA256 _sha = SHA256.Create();

        /// <inheritdoc cref="IHashProvider"/>
        public byte[] Hash(byte[] bytes)
        {
            return _sha.ComputeHash(bytes);
        }
    }
}
#endif