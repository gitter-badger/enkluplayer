using System;
using UnityEngine.UI;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// UI view for displaying license request.
    /// </summary>
    public class MobileLicenseUIView : MonoBehaviourUIElement
    {
        /// <summary>
        /// Details for license request..
        /// </summary>
        public struct LicenseRequestData
        {
            public string Name;
            public string Email;
            public string Phone;
            public string Company;
            public string Story;
        }

        /// <summary>
        /// Email element.
        /// </summary>
        public InputField Email;

        /// <summary>
        /// Name element.
        /// </summary>
        public InputField Name;

        /// <summary>
        /// Phone element.
        /// </summary>
        public InputField Phone;

        /// <summary>
        /// Company element.
        /// </summary>
        public InputField Company;

        /// <summary>
        /// Story element.
        /// </summary>
        public InputField Story;

        /// <summary>
        /// Error element.
        /// </summary>
        public Text Error;

        /// <summary>
        /// Called when requested.
        /// </summary>
        public event Action<LicenseRequestData> OnRequest;
        
        /// <summary>
        /// Called when canceled.
        /// </summary>
        public event Action OnCancel;

        /// <summary>
        /// Called by Unity API when okay is clicked.
        /// </summary>
        public void OnOkayClicked()
        {
            var email = Email.text;
            var name = Name.text;
            var phone = Phone.text;
            var company = Company.text;
            var story = Story.text;

            if (string.IsNullOrEmpty(email))
            {
                Error.text = "Email required.";
                return;
            }
            
            if (string.IsNullOrEmpty(name))
            {
                Error.text = "Name required.";
                return;
            }
            
            if (string.IsNullOrEmpty(story))
            {
                Error.text = "Story required.";
                return;
            }

            if (null != OnRequest)
            {
                OnRequest(new LicenseRequestData
                {
                    Name = name,
                    Email = email,
                    Phone = phone,
                    Company = company,
                    Story = story
                });
            }
        }

        /// <summary>
        /// Called by the Unity API when cancel is clicked.
        /// </summary>
        public void OnCancelClicked()
        {
            if (null != OnCancel)
            {
                OnCancel();
            }
        }
    }
}