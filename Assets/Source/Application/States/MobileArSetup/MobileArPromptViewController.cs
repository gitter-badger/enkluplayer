using System;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Controls the view for the AR prompt.
    /// </summary>
    public class MobileArPromptViewController : MonoBehaviourUIElement
    {
        /// <summary>
        /// Called when the AR service should be started.
        /// </summary>
        public event Action OnStartArService;
        
        /// <summary>
        /// Called by the UI to start the AR service.
        /// </summary>
        public void OnOkay()
        {
            if (null != OnStartArService)
            {
                OnStartArService();
            }
        }
    }
}