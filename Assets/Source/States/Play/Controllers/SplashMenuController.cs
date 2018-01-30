using System;
using CreateAR.SpirePlayer.IUX;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    public class SplashMenuController : MonoBehaviour, IIUXEventDelegate
    {
        private IUXEventHandler _events;
        
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