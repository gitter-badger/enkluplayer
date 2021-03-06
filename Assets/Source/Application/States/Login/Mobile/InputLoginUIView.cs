﻿using System;
using CreateAR.Commons.Unity.Logging;
using UnityEngine;
using UnityEngine.UI;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Controls input view.
    /// </summary>
    public class InputLoginUIView : MonoBehaviourUIElement
    {
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
        /// Called with username and password.
        /// </summary>
        public event Action<string, string> OnSubmit;

        /// <summary>
        /// Called to go to signup.
        /// </summary>
        public event Action OnSignUp;

        /// <summary>
        /// Called to continue as a guest.
        /// </summary>
        public event Action OnContinueAsGuest; 
        
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

        /// <summary>
        /// Called when continue has been clicked.
        /// </summary>
        public void ContinueClicked()
        {
            if (null != OnContinueAsGuest)
            {
                OnContinueAsGuest();
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