using System;
using CreateAR.SpirePlayer.IUX;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Manages the splash menu.
    /// </summary>
    public class SplashMenuController : MonoBehaviour, IIUXEventDelegate
    {
        /// <summary>
        /// Events to listen to.
        /// </summary>
        private IUXEventHandler _events;

        /// <summary>
        /// The container to add elements to.
        /// </summary>
        private Element _container;
        
        /// <summary>
        /// Raw vine.
        /// </summary>
        public VineRawMonoBehaviour Vine;

        /// <summary>
        /// Called when the main menu should be opened.
        /// </summary>
        public event Action OnOpenMenu;

        /// <summary>
        /// Initializes the controller + readies it for show/hide.
        /// </summary>
        /// <param name="events">Events to listen to.</param>
        /// <param name="container">Container to add elements to.</param>
        public void Initialize(IUXEventHandler events, Element container)
        {
            _events = events;
            _container = container;
        }

        /// <summary>
        /// Shows the menu.
        /// </summary>
        public void Show()
        {
            Vine.OnElementCreated += Vine_OnCreated;
            Vine.enabled = true;

            _events.AddHandler(MessageTypes.BUTTON_ACTIVATE, this);
        }

        /// <summary>
        /// Hides the menu.
        /// </summary>
        public void Hide()
        {
            _events.RemoveHandler(MessageTypes.BUTTON_ACTIVATE, this);

            Vine.enabled = false;
            Vine.OnElementCreated -= Vine_OnCreated;
        }

        /// <summary>
        /// Uninitializes controller. Show/Hide should not be called again
        /// until Initialize is called.
        /// </summary>
        public void Uninitialize()
        {
            Hide();
        }

        /// <inheritdoc cref="IIUXEventDelegate"/>
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

        /// <summary>
        /// Called when an element has been created from a vine.
        /// </summary>
        /// <param name="element">The element.</param>
        private void Vine_OnCreated(Element element)
        {
            _container.AddChild(element);
        }
    }
}