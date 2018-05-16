using System;
using UnityEngine.Experimental.UIElements;

namespace CreateAR.SpirePlayer.States.HoloLogin
{
    public class MobileHoloLoginUIView : InjectableMonoBehaviourUIElement
    {
        public Image Image;

        public event Action OnOk;
        
        [Inject]
        public ApplicationConfig Config { get; set; }

        public override void Revealed()
        {
            base.Revealed();

            
        }

        public void OkClicked()
        {
            if (null != OnOk)
            {
                OnOk();
            }
        }
    }
    
    public class HoloLoginApplicationState : IState
    {
        private readonly IUIManager _ui;

        public HoloLoginApplicationState(IUIManager ui)
        {
            _ui = ui;
        }
        
        public void Enter(object context)
        {
            
        }

        public void Update(float dt)
        {
            
        }

        public void Exit()
        {
            
        }
    }
}