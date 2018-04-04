using System;
using UnityEngine;
using UnityEngine.UI;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Controls the view for an AR error.
    /// </summary>
    public class ArErrorViewController : MonoBehaviour
    {
        /// <summary>
        /// Enables camera.
        /// </summary>
        public event Action OnEnableCamera;

        /// <summary>
        /// Called by the UI when the OK button is touched.
        /// </summary>
        public void OnOkay()
        {
            if (null != OnEnableCamera)
            {
                OnEnableCamera();
            }   
        }
    }
}