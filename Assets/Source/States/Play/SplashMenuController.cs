using System;
using CreateAR.Commons.Unity.Messaging;
using CreateAR.SpirePlayer.IUX;

namespace CreateAR.SpirePlayer
{
    public class SplashMenuController : InjectableMonoBehaviour, IIUXEventHandler
    {
        private IUXEventHandler _events;

        [Inject]
        public IMessageRouter Messages { get; set; }

        public VineRawMonoBehaviour Vine;

        public event Action OnOpenMenu;
        
        public void Initialize(IUXEventHandler events)
        {
            _events = events;
        }

        public void Show()
        {
            Vine.enabled = true;

            _events.AddHandler(MessageTypes.BUTTON_ACTIVATE, this);
        }

        public void Hide()
        {
            _events.RemoveHandler(MessageTypes.BUTTON_ACTIVATE, this);

            Vine.enabled = false;
        }

        public void Uninitialize()
        {
            
        }

        public bool OnEvent(IUXEvent @event)
        {
            if ("btn-menu" == @event.Target.Id)
            {
                if (null != OnOpenMenu)
                {
                    OnOpenMenu();
                }

                return true;
            }

            return false;
        }
    }
}