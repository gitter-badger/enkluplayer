using System;
using CreateAR.SpirePlayer.IUX;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Generic error popup.
    /// </summary>
    public class ErrorPopupUIView : MonoBehaviourIUXController, ICommonErrorView
    {
        /// <summary>
        /// Elements.
        /// </summary>
        [InjectElements("..cpn-error")]
        public CaptionWidget CpnError { get; set; }
        [InjectElements("..btn-ok")]
        public ButtonWidget BtnOk { get; set; }

        /// <summary>
        /// Called when the ok button has been pressed.
        /// </summary>
        public event Action OnOk;

        /// <summary>
        /// Error message.
        /// </summary>
        public string Message
        {
            get
            {
                return CpnError.Schema.Get<string>("label").Value;
            }
            set
            {
                CpnError.Schema.Set("label", value);
            }
        }

        /// <summary>
        /// The label on the button.
        /// </summary>
        public string Action
        {
            get
            {
                return BtnOk.Schema.Get<string>("label").Value;
            }
            set
            {
                BtnOk.Schema.Set("label", value);
            }
        }

        /// <summary>
        /// MonoBehaviour.
        /// </summary>
        protected override void AfterElementsCreated()
        {
            base.AfterElementsCreated();

            BtnOk.Activator.OnActivated += _ =>
            {
                if (null != OnOk)
                {
                    OnOk();
                }
            };
        }
    }
}