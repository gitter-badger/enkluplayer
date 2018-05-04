using System;
using CreateAR.SpirePlayer.IUX;

namespace CreateAR.SpirePlayer
{
    public class ErrorPopup : MonoBehaviourIUXController
    {
        [InjectElements("..cpn-error")]
        public CaptionWidget CpnError { get; set; }

        [InjectElements("..btn-ok")]
        public ButtonWidget BtnOk { get; set; }

        public event Action OnOk;

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

        protected override void Awake()
        {
            base.Awake();

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