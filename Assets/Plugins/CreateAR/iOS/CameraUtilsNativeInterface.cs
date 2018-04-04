using System.Runtime.InteropServices;

namespace CreateAR.SpirePlayer
{
	/// <summary>
	/// Native interface for IOS.
	/// </summary>
	public static class CameraUtilsNativeInterface
	{
#if UNITY_IOS && !UNITY_EDITOR
		[DllImport("__Internal")]
	    public static extern bool unity_hasCameraPermissions();

		[DllImport("__Internal")]
	    public static extern void unity_openSettings();
#endif

		/// <summary>
		/// Retrieves whether or not the application has permission to use the camera.
		/// </summary>
		public static bool HasCameraPermissions
		{
			get
			{
#if UNITY_IOS && !UNITY_EDITOR
				return unity_hasCameraPermissions();
#endif
				return true;
			}
		}
		
		/// <summary>
		/// Requests permission for the app to use the camera.
		/// </summary>
		public static void OpenSettings()
		{
#if UNITY_IOS && !UNITY_EDITOR
			unity_openSettings();		
#endif
		}
	}
}
