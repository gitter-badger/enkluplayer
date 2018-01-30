using System;
using CreateAR.SpirePlayer.IUX;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    public class MainMenuController : MonoBehaviour, IIUXEventDelegate
    {
        private IUXEventHandler _events;

        public VineRawMonoBehaviour Vine;

        public event Action OnBack;
        public event Action OnPlay;
        public event Action OnClearAll;
        public event Action OnQuit;
        public event Action<bool> OnDebugRender;

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
            switch (id)
            {
                case "menu.btn-back":
                {
                    if (null != OnBack)
                    {
                        OnBack();
                    }

                    return true;
                }
                case "btn-clearall":
                {
                    if (null != OnClearAll)
                    {
                        OnClearAll();
                    }

                    return true;
                }
                case "btn-quit":
                {
                    if (null != OnQuit)
                    {
                        OnQuit();
                    }

                    return true;
                }
            }
            
            return false;
        }
    }
}