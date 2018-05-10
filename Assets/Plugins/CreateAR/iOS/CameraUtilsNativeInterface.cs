using System;
using System.Collections;
using System.Runtime.InteropServices;
using CreateAR.Commons.Unity.Http;
using UnityEngine;

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
	    public static extern bool unity_hasDeniedCameraPermissions();

		[DllImport("__Internal")]
	    public static extern bool unity_requestCameraPermissions();
		
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
#else
				return true;				
#endif
			}
		}
		
		/// <summary>
		/// Retrieves whether or not the user has specifically denied access to the camera.
		/// </summary>
		public static bool HasDeniedCameraPermissions
		{
			get
			{
#if UNITY_IOS && !UNITY_EDITOR
				return unity_hasDeniedCameraPermissions();
#else
				return false;
#endif
			}
		}
		
		/// <summary>
		/// Requests permission for the app to use the camera.
		/// </summary>
		public static void RequestCameraAccess(
			IBootstrapper bootstrapper,
			Action<bool> callback)
		{
#if UNITY_IOS && !UNITY_EDITOR
			unity_requestCameraPermissions();		
#endif
			
			bootstrapper.BootstrapCoroutine(CheckPermissions(callback));
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
		
		/// <summary>
		/// Polls for permissions change.
		/// </summary>
		/// <param name="callback">The callback to call.</param>
		/// <returns></returns>
		private static IEnumerator CheckPermissions(Action<bool> callback)
		{
			while (true)
			{
#if UNITY_IOS && !UNITY_EDITOR
				if (unity_hasCameraPermissions())
				{
					callback(true);
					yield break;
				}
				else if (unity_hasDeniedCameraPermissions())
				{
					callback(false);
					yield break;
				}
#else
				callback(true);
				yield break;
#endif
			}
		}
	}
}
