using System;
using UnityEngine;
using UnityEngine.UI;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Controls view for an apps list element.
    /// </summary>
    public class AppsListElementController : MonoBehaviour
    {
        /// <summary>
        /// The app id.
        /// </summary>
        private string _appId;

        /// <summary>
        /// Title field.
        /// </summary>
        public Text Title;

        /// <summary>
        /// Description field.
        /// </summary>
        public Text Description;
        
        /// <summary>
        /// Called when selected.
        /// </summary>
        public event Action<string> OnSelected;

        /// <summary>
        /// Initializes the element.
        /// </summary>
        public void Init(string appId, string name, string description)
        {
            _appId = appId;
            
            Title.text = name;
            Description.text = description;
        }

        /// <summary>
        /// Called by Unity's UI when a button has been pressed.
        /// </summary>
        public void OnButton()
        {
            if (null != OnSelected)
            {
                OnSelected(_appId);
            }
        }
    }
}