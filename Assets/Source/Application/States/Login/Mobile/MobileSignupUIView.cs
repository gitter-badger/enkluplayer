using System;
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
        public InputField Email;

        /// <summary>
        /// Display name.
        /// </summary>
        public InputField DisplayName;
        
        /// <summary>
        /// Password.
        /// </summary>
        public InputField Password;
        
        /// <summary>
        /// License key.
        /// </summary>
        public InputField LicenseKey;

        /// <summary>
        /// Error box.
        /// </summary>
        public Text Error;

        /// <summary>
        /// Called when signup is requested.
        /// </summary>
        public event Action<SignupRequestData> OnSubmit;

        /// <summary>
        /// Called when license key info is requested.
        /// </summary>
        public event Action OnLicenseInfo;

        /// <summary>
        /// Called when login is requested.
        /// </summary>
        public event Action OnLogin;

        /// <summary>
        /// Called when continue as guest is clicked.
        /// </summary>
        public event Action OnContinueAsGuest;

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
            
            if (string.IsNullOrEmpty(displayName))
            {
                Error.text = "Display name required.";
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
            
            if (null != OnSubmit)
            {
                OnSubmit(new SignupRequestData
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

        /// <summary>
        /// Called when login has been clicked.
        /// </summary>
        public void LoginClicked()
        {
            if (null != OnLogin)
            {
                OnLogin();
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
    }
}