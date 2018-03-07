using System;
using CreateAR.Commons.Unity.Messaging;
using UnityEngine;
using UnityEngine.UI;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Displays application status.
    /// </summary>
    public class StatusDisplayBehaviour : InjectableMonoBehaviour
    {
        /// <summary>
        /// Unsubbscribe.
        /// </summary>
        private Action _unsub;

        /// <summary>
        /// Absolute time at which status should be cleared.
        /// </summary>
        private long _timeToClear;

        /// <summary>
        /// Dependencies.
        /// </summary>
        [Inject]
        public IMessageRouter Router { get; set; }

        /// <summary>
        /// Text component to render with.
        /// </summary>
        public Text Text;

        /// <inheritdoc cref="MonoBehaviour"/>
        private void OnEnable()
        {
            _unsub = Router.Subscribe(MessageTypes.STATUS, Messages_OnStatus);
        }

        /// <inheritdoc cref="MonoBehaviour"/>
        private void OnDestroy()
        {
            _unsub();
        }

        /// <inheritdoc cref="MonoBehaviour"/>
        private void LateUpdate()
        {
            if (_timeToClear > 0)
            {
                if (DateTime.Now.Ticks >= _timeToClear)
                {
                    _timeToClear = 0;

                    Text.text = "";
                    Text.enabled = false;
                }
            }
        }

        /// <summary>
        /// Called when a status event is received.
        /// </summary>
        /// <param name="status"></param>
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
            Text.enabled = !string.IsNullOrEmpty(status.ToString());
        }
    }
}