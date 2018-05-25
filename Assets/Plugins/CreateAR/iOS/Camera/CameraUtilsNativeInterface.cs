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
	    public static extern bool unity_hasPhotosPermissions();
	
		[DllImport("__Internal")]
	    public static extern bool unity_hasDeniedPhotosPermissions();

		[DllImport("__Internal")]
	    public static extern bool unity_requestPhotosPermissions();
		
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
			
			bootstrapper.BootstrapCoroutine(CheckCameraPermissions(callback));
		}
		
		/// <summary>
		/// Retrieves whether or not the application has permission to use the camera.
		/// </summary>
		public static bool HasPhotosPermissions
		{
			get
			{
#if UNITY_IOS && !UNITY_EDITOR
				return unity_hasPhotosPermissions();
#else
				return true;				
#endif
			}
		}
		
		/// <summary>
		/// Retrieves whether or not the user has specifically denied access to the camera.
		/// </summary>
		public static bool HasDeniedPhotosPermissions
		{
			get
			{
#if UNITY_IOS && !UNITY_EDITOR
				return unity_hasDeniedPhotosPermissions();
#else
				return false;
#endif
			}
		}
		
		/// <summary>
		/// Requests permission for the app to use the camera.
		/// </summary>
		public static void RequestPhotosAccess(
			IBootstrapper bootstrapper,
			Action<bool> callback)
		{
#if UNITY_IOS && !UNITY_EDITOR
			unity_requestPhotosPermissions();		
#endif
			
			bootstrapper.BootstrapCoroutine(CheckPhotosPermissions(callback));
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
		private static IEnumerator CheckCameraPermissions(Action<bool> callback)
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
		
		/// <summary>
		/// Polls for permissions change.
		/// </summary>
		/// <param name="callback">The callback to call.</param>
		/// <returns></returns>
		private static IEnumerator CheckPhotosPermissions(Action<bool> callback)
		{
			while (true)
			{
#if UNITY_IOS && !UNITY_EDITOR
				if (unity_hasPhotosPermissions())
				{
					callback(true);
					yield break;
				}
				else if (unity_hasDeniedPhotosPermissions())
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
