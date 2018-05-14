using System;
using JetBrains.Annotations;
using UnityEngine.UI;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Signup view for mobile.
    /// </summary>
    public class MobileSignupUIView : MonoBehaviourUIElement
    {
        /// <summary>
        /// Data for the request.
        /// </summary>
        public struct SignupRequestData
        {
            public string Email;
            public string DisplayName;
            public string Password;
            public string LicenseKey;
        }
        
        /// <summary>
        /// Email.
        /// </summary>
        public Text Email;

        /// <summary>
        /// Display name.
        /// </summary>
        public Text DisplayName;
        
        /// <summary>
        /// Password.
        /// </summary>
        public Text Password;
        
        /// <summary>
        /// License key.
        /// </summary>
        public Text LicenseKey;

        /// <summary>
        /// Error box.
        /// </summary>
        public Text Error;

        /// <summary>
        /// Called when signup is requested.
        /// </summary>
        public event Action<SignupRequestData> OnSignUp;

        /// <summary>
        /// Called when license key info is requested.
        /// </summary>
        public event Action OnLicenseInfo;

        /// <summary>
        /// Called when login has been clicked.
        /// </summary>
        public void SignupClicked()
        {
            var email = Email.text;
            var displayName = DisplayName.text;
            var password = Password.text;
            var license = LicenseKey.text;
            
            // validate
            if (string.IsNullOrEmpty(email))
            {
                Error.text = "Email address required.";
                return;
            }
            
            if (string.IsNullOrEmpty(password) || password.Length < 6)
            {
                Error.text = "Password of length 6 or more required.";
                return;
            }
            
            if (string.IsNullOrEmpty(license))
            {
                Error.text = "License required.";
                return;
            }
            
            if (string.IsNullOrEmpty(displayName))
            {
                Error.text = "Display name required.";
                return;
            }
            
            if (null != OnSignUp)
            {
                OnSignUp(new SignupRequestData
                {
                    Email = email,
                    LicenseKey = license,
                    Password = password,
                    DisplayName = displayName
                });
            }
        }

        /// <summary>
        /// Called when license info has been clicked.
        /// </summary>
        public void LicenseInfoClicked()
        {
            if (null != OnLicenseInfo)
            {
                OnLicenseInfo();
            }
        }
    }
}