namespace CreateAR.SpirePlayer.Qr
{
    /// <summary>
    /// Ios interface.
    /// </summary>
    public class IosQrNativeInterface
    {
#if UNITY_IOS && !UNITY_EDITOR
        [System.Runtime.InteropServices.DllImport("__Internal")]
        public static extern string unity_startDecoding(string path);
#endif

        /// <summary>
        /// Attempts to decode at path.
        /// </summary>
        /// <param name="path">Path to the image.</param>
        /// <returns></returns>
        public static string DecodeAtPath(string path)
        {
#if UNITY_IOS && !UNITY_EDITOR
            return unity_startDecoding(path);
#else
            return string.Empty;
#endif
        }
    }
}