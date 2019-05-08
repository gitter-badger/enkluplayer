using System;
using CreateAR.EnkluPlayer.IUX;

namespace CreateAR.EnkluPlayer.Player.Session
{
    /// <summary>
    /// View controller for scanning QR codes from mobile device to create a session.
    /// </summary>
    public class SessionQrViewController : MonoBehaviourIUXController
    {
        /// <summary>
        /// Elements.
        /// </summary>
        [InjectElements("..img-qr")]
        public ImageWidget Qr { get; set; }
        [InjectElements("..caption-progress")]
        public TextWidget Progress { get; set; }

        /// <inheritdoc />
        protected override void AfterElementsCreated()
        {
            base.AfterElementsCreated();
            
        }

        /// <summary>
        /// Shows a message instead of the QR image.
        /// </summary>
        /// <param name="message">The message to show.</param>
        public void ShowMessage(string message)
        {
            Qr.Schema.Set("visible", false);

            Progress.Schema.Set("label", message);
            Progress.Schema.Set("visible", true);
        }
    }
}
