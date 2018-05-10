using System;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Play mode menu for mobile.
    /// </summary>
    public class MobilePlayModeMenu : MonoBehaviourUIElement
    {
        /// <summary>
        /// Called when insta button has been clicked.
        /// </summary>
        public event Action OnInstaClicked;

        /// <summary>
        /// Called when back button has been clicked.
        /// </summary>
        public event Action OnBackClicked;

        /// <summary>
        /// Called by Unity when back is clicked.
        /// </summary>
        public void BackClicked()
        {
            if (null != OnBackClicked)
            {
                OnBackClicked();
            }
        }
        
        /// <summary>
        /// Called by Unity when insta is clicked.
        /// </summary>
        public void InstaClicked()
        {
            if (null != OnInstaClicked)
            {
                OnInstaClicked();
            }
        }
    }
}