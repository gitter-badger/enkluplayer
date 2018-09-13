#if !UNITY_EDITOR && UNITY_WSA

using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;

namespace CreateAR.EnkluPlayer.Assets
{
    /// <summary>
    /// UWP implementation.
    /// </summary>
    public class ShaUwpHashProvider : IHashProvider
    {
        /// <inheritdoc cref="IHashProvider"/>
        public byte[] Hash(byte[] bytes)
        {
            var provider = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha256);
            var inBuffer = CryptographicBuffer.CreateFromByteArray(bytes);
            var outBuffer = provider.HashData(inBuffer);

            CryptographicBuffer.CopyToByteArray(outBuffer, out byte[] outBytes);

            return outBytes;
        }
    }
}
#endif