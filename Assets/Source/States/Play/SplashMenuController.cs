using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.IUX;

namespace CreateAR.SpirePlayer
{
    public class SplashMenuController : InjectableMonoBehaviour, IIUXEventHandler
    {
        private IUXEventHandler _events;

        public void Initialize(IUXEventHandler events)
        {
            _events = events;
            _events.AddHandler(MessageTypes.BUTTON_ACTIVATE, this);
        }

        public void Uninitialize()
        {
            _events.RemoveHandler(MessageTypes.BUTTON_ACTIVATE, this);
        }

        public bool OnEvent(IUXEvent @event)
        {
            Log.Info(this, "OnEvent({0})", @event.Target.Id);

            return false;
        }
    }
}