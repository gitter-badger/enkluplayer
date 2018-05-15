namespace CreateAR.SpirePlayer.Qr
{
    public class IosQrNativeInterface
    {
#if UNITY_IOS && !UNITY_EDITOR
        [System.Runtime.InteropServices.DllImport("__Internal")]
        public static extern string unity_startDecoding(string path);
#endif
    }
}