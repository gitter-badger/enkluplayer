using System;
using CreateAR.SpirePlayer.IUX;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    public class MainMenuController : MonoBehaviour, IIUXEventDelegate
    {
        private IUXEventHandler _events;
        private Element _container;

        public VineRawMonoBehaviour Vine;

        public event Action OnBack;
        public event Action OnPlay;
        public event Action OnNew;
        public event Action OnClearAll;
        public event Action OnQuit;
        public event Action<bool> OnDebugRender;

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
                case "btn-new":
                {
                    if (null != OnNew)
                    {
                        OnNew();
                    }

                    return true;
                }
            }
            
            return false;
        }

        private void Vine_OnCreated(Element element)
        {
            _container.AddChild(element);
        }
    }
}