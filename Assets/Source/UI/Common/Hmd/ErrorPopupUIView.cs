using System;
using CreateAR.EnkluPlayer.IUX;

namespace CreateAR.EnkluPlayer
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
        public TextWidget CpnError { get; set; }
        [InjectElements("..btn-ok")]
        public ButtonWidget BtnOk { get; set; }

        /// <inheritdoc />
        public event Action OnOk;

        /// <inheritdoc />
        public void DisableAction()
        {
            BtnOk.Schema.Set("visible", false);
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
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