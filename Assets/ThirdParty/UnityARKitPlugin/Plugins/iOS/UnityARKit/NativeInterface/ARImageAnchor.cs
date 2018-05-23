﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.InteropServices;


namespace UnityEngine.XR.iOS
{
	
	public struct UnityARImageAnchorData 
	{

		public IntPtr ptrIdentifier;

		/**
	 		The transformation matrix that defines the anchor's rotation, translation and scale in world coordinates.
			 */
		public UnityARMatrix4x4 transform;

		public IntPtr referenceImageNamePtr;

		public float referenceImagePhysicalSize;


	};



	public class ARImageAnchor {

		private UnityARImageAnchorData imageAnchorData;

		public ARImageAnchor (UnityARImageAnchorData uiad)
		{
			imageAnchorData = uiad;
		}


	    public string identifier
	    {
	        get
	        {
#if !NETFX_CORE
                return Marshal.PtrToStringAuto(imageAnchorData.ptrIdentifier);
#else
                return string.Empty;
#endif
            }
	    }

	    public Matrix4x4 transform { 
			get { 
				Matrix4x4 matrix = new Matrix4x4 ();
				matrix.SetColumn (0, imageAnchorData.transform.column0);
				matrix.SetColumn (1, imageAnchorData.transform.column1);
				matrix.SetColumn (2, imageAnchorData.transform.column2);
				matrix.SetColumn (3, imageAnchorData.transform.column3);
				return matrix;
			}
		}

	    public string referenceImageName
	    {
	        get
	        {
#if !NETFX_CORE
                return Marshal.PtrToStringAuto(imageAnchorData.referenceImageNamePtr);
#else
                return string.Empty;
#endif
            }
	    }

	    public float referenceImagePhysicalSize { get { return imageAnchorData.referenceImagePhysicalSize; } }
	}

}