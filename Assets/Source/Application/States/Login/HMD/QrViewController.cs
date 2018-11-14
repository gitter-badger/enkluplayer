using System;
using CreateAR.EnkluPlayer.IUX;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// View controller for scanning QR codes.
    /// </summary>
    public class QrViewController : MonoBehaviourIUXController
    {
        /// <summary>
        /// Elements.
        /// </summary>
        [InjectElements("..img-qr")]
        public ImageWidget Qr { get; set; }
        [InjectElements("..caption-progress")]
        public CaptionWidget Progress { get; set; }
        [InjectElements("..(@type==ButtonWidget)")]
        public ButtonWidget Btn { get; set; }

        /// <summary>
        /// Fired when config button has been pressed.
        /// </summary>
        public event Action OnConfigure;

        /// <inheritdoc />
        protected override void AfterElementsCreated()
        {
            base.AfterElementsCreated();

            Btn.OnActivated += _ =>
            {
                if (null != OnConfigure)
                {
                    OnConfigure();
                }
            };
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