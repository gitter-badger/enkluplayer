using System;
using CreateAR.Trellis.Messages.GetMyApps;
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
        /// The app.
        /// </summary>
        private Body _app;

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
        /// <param name="app">The app to display info for.</param>
        public void Init(Body app)
        {
            _app = app;

            Title.text = _app.Name;
            Description.text = _app.Description;
        }

        /// <summary>
        /// Called by Unity's UI when a button has been pressed.
        /// </summary>
        public void OnButton()
        {
            if (null != OnSelected)
            {
                OnSelected(_app.Id);
            }
        }
    }
}