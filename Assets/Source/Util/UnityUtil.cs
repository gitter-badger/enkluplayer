using UnityEngine;

namespace CreateAR.EnkluPlayer.Util
{
    /// <summary>
    /// Utilities for things that are difficult in Unity.
    /// </summary>
    public static class UnityUtil
    {
        /// <summary>
        /// Retrieves the current platform which is not straightforward.
        /// </summary>
        /// <returns></returns>
        public static string CurrentPlatform()
        {
#if UNITY_EDITOR
            switch (UnityEditor.EditorUserBuildSettings.activeBuildTarget)
            {
                case UnityEditor.BuildTarget.Android:
                {
                    return RuntimePlatform.Android.ToString();
                }
                case UnityEditor.BuildTarget.iOS:
                {
                    return RuntimePlatform.IPhonePlayer.ToString();
                }
                case UnityEditor.BuildTarget.WebGL:
                {
                    return RuntimePlatform.WebGLPlayer.ToString();
                }
                case UnityEditor.BuildTarget.WSAPlayer:
                {
                    return RuntimePlatform.WSAPlayerX86.ToString();
                }
                default:
                {
                    return UnityEngine.Application.platform.ToString();
                }
            }
#else
            switch (UnityEngine.Application.platform)
            {
                case RuntimePlatform.WSAPlayerARM:
                case RuntimePlatform.WSAPlayerX64:
                case RuntimePlatform.WSAPlayerX86:
                {
                    return "WSAPlayerX86";
                }
                default:
                {
                    return UnityEngine.Application.platform.ToString();
                }
            }
#endif
        }
    }
}
