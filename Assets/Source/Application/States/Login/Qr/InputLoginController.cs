using System;
using UnityEngine;
using UnityEngine.UI;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Controls input view.
    /// </summary>
    public class InputLoginController : MonoBehaviour
    {
        /// <summary>
        /// Called with username and password.
        /// </summary>
        public event Action<string, string> OnSubmit;

        /// <summary>
        /// The username field.
        /// </summary>
        public InputField UserName;
        
        /// <summary>
        /// The password field.
        /// </summary>
        public InputField Password;

        /// <summary>
        /// Error field.
        /// </summary>
        public Text Error;

        /// <summary>
        /// Called when submit button has been pressed.
        /// </summary>
        public void Submit_OnClicked()
        {
            if (null != OnSubmit)
            {
                OnSubmit(UserName.text, Password.text);
            }
        }

        /// <inheritdoc cref="MonoBehaviour"/>
        private void OnDisable()
        {
            Error.text = string.Empty;
        }
    }
}