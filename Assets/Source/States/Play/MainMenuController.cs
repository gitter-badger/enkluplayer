using System;
using CreateAR.SpirePlayer.IUX;

namespace CreateAR.SpirePlayer
{
    public class MainMenuController : InjectableMonoBehaviour, IIUXEventHandler
    {
        private IUXEventHandler _events;

        public VineRawMonoBehaviour Vine;

        public event Action OnBack;

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
            //
        }

        public bool OnEvent(IUXEvent @event)
        {
            var id = @event.Target.Id;
            if ("menu.back" == id)
            {
                if (null != OnBack)
                {
                    OnBack();
                }

                return true;
            }

            return false;
        }
    }
}