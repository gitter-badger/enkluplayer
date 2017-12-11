#if !UNITY_EDITOR && UNITY_WSA
namespace CreateAR.SpirePlayer.Assets
{
    public class ShaUwpHashProvider : IHashProvider
    {
        public byte[] Hash(byte[] bytes)
        {
            return new byte[0];
        }
    }
}
#endif