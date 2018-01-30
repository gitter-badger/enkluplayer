using System;
using CreateAR.SpirePlayer.IUX;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    public class QuitController : MonoBehaviour, IIUXEventDelegate
    {
        private IUXEventHandler _events;
        private Element _container;

        public VineRawMonoBehaviour Vine;

        public event Action OnConfirm;
        public event Action OnCancel;

        public void Initialize(IUXEventHandler events, Element container)
        {
            _events = events;
            _container = container;
        }

        public void Show()
        {
            Vine.OnElementCreated += Vine_OnCreated;
            Vine.enabled = true;

            _events.AddHandler(MessageTypes.BUTTON_ACTIVATE, this);
        }

        public void Hide()
        {
            _events.RemoveHandler(MessageTypes.BUTTON_ACTIVATE, this);

            Vine.enabled = false;
            Vine.OnElementCreated -= Vine_OnCreated;
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

        private void Vine_OnCreated(Element element)
        {
            _container.AddChild(element);
        }
    }
}