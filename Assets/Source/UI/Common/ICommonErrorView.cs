using System;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Error popup.
    /// </summary>
    public interface ICommonErrorView : IUIElement
    {
        /// <summary>
        /// Message to display.
        /// </summary>
        string Message { get; set; }
        
        /// <summary>
        /// Name of action.
        /// </summary>
        string Action { get; set; }
        
        /// <summary>
        /// Okay button.
        /// </summary>
        event Action OnOk;

        /// <summary>
        /// Disables action button.
        /// </summary>
        void DisableAction();
    }
}