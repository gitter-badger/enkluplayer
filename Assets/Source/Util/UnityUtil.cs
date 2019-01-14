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
        public static RuntimePlatform CurrentPlatform()
        {
#if UNITY_EDITOR
            switch (UnityEditor.EditorUserBuildSettings.activeBuildTarget)
            {
                case UnityEditor.BuildTarget.Android:
                {
                    return RuntimePlatform.Android;
                }
                case UnityEditor.BuildTarget.iOS:
                {
                    return RuntimePlatform.IPhonePlayer;
                }
                case UnityEditor.BuildTarget.WebGL:
                {
                    return RuntimePlatform.WebGLPlayer;
                }
                case UnityEditor.BuildTarget.WSAPlayer:
                {
                    return RuntimePlatform.WSAPlayerX86;
                }
                default:
                {
                    return UnityEngine.Application.platform;
                }
            }
#else
                return UnityEngine.Application.platform;
#endif
        }
    }
}
