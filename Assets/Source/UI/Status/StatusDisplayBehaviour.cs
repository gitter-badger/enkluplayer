using System;
using CreateAR.Commons.Unity.Messaging;
using UnityEngine;
using UnityEngine.UI;

namespace CreateAR.SpirePlayer
{
    public class StatusDisplayBehaviour : InjectableMonoBehaviour
    {
        private Action _unsub;
        private long _timeToClear;

        [Inject]
        public IMessageRouter Router { get; set; }

        public Text Text;

        private void OnEnable()
        {
            _unsub = Router.Subscribe(MessageTypes.STATUS, Messages_OnStatus);
        }

        private void OnDestroy()
        {
            _unsub();
        }

        private void LateUpdate()
        {
            if (_timeToClear > 0)
            {
                if (DateTime.Now.Ticks >= _timeToClear)
                {
                    _timeToClear = 0;

                    Text.text = "";
                }
            }
        }

        private void Messages_OnStatus(object status)
        {
            var @event = status as StatusEvent;

            if (null != @event)
            {
                if (@event.DurationSeconds > Mathf.Epsilon)
                {
                    _timeToClear = 0;
                }
                else
                {
                    _timeToClear = DateTime.Now.Ticks + (long) (TimeSpan.TicksPerSecond * @event.DurationSeconds);
                }
            }
            else
            {
                _timeToClear = 0;
            }

            Text.text = status.ToString();
        }
    }
}
