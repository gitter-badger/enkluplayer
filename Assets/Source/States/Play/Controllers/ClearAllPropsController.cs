using System;
using CreateAR.SpirePlayer.IUX;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    public class ClearAllPropsController : MonoBehaviour, IIUXEventDelegate
    {
        private IUXEventHandler _events;

        public VineRawMonoBehaviour Vine;

        public event Action OnConfirm;
        public event Action OnCancel;

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
            if ("btn-yes" == @event.Target.Id)
            {
                if (null != OnConfirm)
                {
                    OnConfirm();
                }

                return true;
            }

            if ("btn-no" == @event.Target.Id)
            {
                if (null != OnCancel)
                {
                    OnCancel();
                }

                return true;
            }

            return false;
        }
    }
}