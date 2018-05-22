using System;
using CreateAR.Commons.Unity.Logging;
using UnityEngine;
using UnityEngine.UI;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Controls input view.
    /// </summary>
    public class InputLoginUIView : MonoBehaviourUIElement
    {
        /// <summary>
        /// Called with username and password.
        /// </summary>
        public event Action<string, string> OnSubmit;

        /// <summary>
        /// Called to go to signup.
        /// </summary>
        public event Action OnSignUp;

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
            if (string.IsNullOrEmpty(UserName.text))
            {
                Error.text = "Please enter a valid email.";
                return;
            }

            if (string.IsNullOrEmpty(Password.text))
            {
                Error.text = "Please enter a valid password.";
                return;
            }
            
            if (null != OnSubmit)
            {
                OnSubmit(UserName.text, Password.text);
            }
        }
    
        /// <summary>
        /// Called when the signup button has been pressed.
        /// </summary>
        public void OnSignupClicked()
        {
            if (null != OnSignUp)
            {
                OnSignUp();
            }
        }

        /// <inheritdoc />
        public override void Removed()
        {
            base.Removed();
        
            Error.text = string.Empty;
        }
    }
}