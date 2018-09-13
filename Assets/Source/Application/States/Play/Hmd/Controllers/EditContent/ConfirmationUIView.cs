using System;
using CreateAR.EnkluPlayer.IUX;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Confirmation dialog.
    /// </summary>
    public class ConfirmationUIView : MonoBehaviourIUXController
    {
        /// <summary>
        /// Elements.
        /// </summary>
        [InjectElements("..btn-confirm")]
        public ButtonWidget BtnConfirm { get; set; }

        [InjectElements("..btn-cancel")]
        public ButtonWidget BtnCancel { get; set; }

        [InjectElements("..cpn-text")]
        public CaptionWidget CpnText { get; set; }

        /// <summary>
        /// Called when confirm is pressed.
        /// </summary>
        public event Action OnConfirm;

        /// <summary>
        /// Called when cancel is pressed.
        /// </summary>
        public event Action OnCancel;

        /// <summary>
        /// Gets/sets the message.
        /// </summary>
        public string Message
        {
            get { return CpnText.Label; }
            set { CpnText.Label = value; }
        }

        /// <inheritdoc />
        protected override void AfterElementsCreated()
        {
            base.AfterElementsCreated();

            BtnConfirm.Activator.OnActivated += _ =>
            {
                if (null != OnConfirm)
                {
                    OnConfirm();
                }
            };

            BtnCancel.Activator.OnActivated += _ =>
            {
                if (null != OnCancel)
                {
                    OnCancel();
                }
            };
        }
    }
}