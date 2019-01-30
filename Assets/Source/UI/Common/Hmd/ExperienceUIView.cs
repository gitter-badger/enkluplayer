using System;
using CreateAR.EnkluPlayer.IUX;

namespace CreateAR.EnkluPlayer
{
    public class ExperienceUIView : MonoBehaviourIUXController
    {
        
        
        [InjectElements("..btn-close")]
        public ButtonWidget BtnClose { get; set; }
        
        [InjectElements("..txt-environment")]
        public TextWidget TxtEnvironment { get; set; }
        
        [InjectElements("..txt-experience")]
        public TextWidget TxtExperience { get; set; }

        public event Action OnClose;

        protected override void AfterElementsCreated()
        {
            base.AfterElementsCreated();

            BtnClose.OnActivated += _ =>
            {
                if (OnClose != null)
                {
                    OnClose();
                }
            };
        }
    }
}