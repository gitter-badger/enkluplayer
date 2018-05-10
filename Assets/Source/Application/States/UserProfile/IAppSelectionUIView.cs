using System;
using CreateAR.Trellis.Messages.GetMyApps;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Describes a UI that allows a user to select the app to load into.
    /// </summary>
    public interface IAppSelectionUIView : IUIElement
    {
        /// <summary>
        /// Called when an app has been selected.
        /// </summary>
        event Action<string> OnAppSelected;
        
        /// <summary>
        /// Sets the apps to display.
        /// </summary>
        Body[] Apps { get; set; }
    }
}