using System;
using CreateAR.SpirePlayer.IUX;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Manages quit controller.
    /// </summary>
    public class QuitController : MonoBehaviour, IIUXEventDelegate
    {
        /// <summary>
        /// Events to listen to.
        /// </summary>
        private IUXEventHandler _events;

        /// <summary>
        /// Container to add elements to.
        /// </summary>
        private Element _container;

        /// <summary>
        /// The raw vine.
        /// </summary>
        public VineRawMonoBehaviour Vine;

        /// <summary>
        /// Called to confirm quitting.
        /// </summary>
        public event Action OnConfirm;

        /// <summary>
        /// Called to cancel.
        /// </summary>
        public event Action OnCancel;

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

        /// <summary>
        /// Called when an element is created from a vine.
        /// </summary>
        /// <param name="element">The element.</param>
        private void Vine_OnCreated(Element element)
        {
            _container.AddChild(element);
        }
    }
}