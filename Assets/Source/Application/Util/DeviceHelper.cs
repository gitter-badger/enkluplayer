namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Various device related things.
    /// </summary>
    public static class DeviceHelper
    {
        /// <summary>
        /// True iff the device is a HoloLens.
        /// </summary>
        /// <returns></returns>
        public static bool IsHoloLens()
        {
#if NETFX_CORE
            return true;
#else
            return false;
#endif
        }

        /// <summary>
        /// True iff the platform is WebGL.
        /// </summary>
        /// <returns></returns>
        public static bool IsWebGl()
        {
#if UNITY_WEBGL
            return true;
#else
            return false;
#endif
        }
    }
}