using System;
using CreateAR.SpirePlayer.IUX;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Manages the main menu.
    /// </summary>
    public class MainMenuController : MonoBehaviour, IIUXEventDelegate
    {
        /// <summary>
        /// Handles events.
        /// </summary>
        private IUXEventHandler _events;

        /// <summary>
        /// Container element to add to.
        /// </summary>
        private Element _container;

        /// <summary>
        /// The raw vine.
        /// </summary>
        public VineRawMonoBehaviour Vine;

        /// <summary>
        /// Called when we wish to go back.
        /// </summary>
        public event Action OnBack;
        
        /// <summary>
        /// Called when the play button is pressed.
        /// </summary>
        public event Action OnPlay;

        /// <summary>
        /// Called when the new button is pressed.
        /// </summary>
        public event Action OnNew;

        /// <summary>
        /// Called when the clearall button is pressed.
        /// </summary>
        public event Action OnClearAll;

        /// <summary>
        /// Called when the quit button is pressed.
        /// </summary>
        public event Action OnQuit;

        /// <summary>
        /// Called when the DebugRender button is pressed.
        /// </summary>
        public event Action<bool> OnDebugRender;

        /// <summary>
        /// Makes the controller ready for show/hide.
        /// </summary>
        /// <param name="events">Events to listen to.</param>
        /// <param name="container">Container element to add to.</param>
        public void Initialize(IUXEventHandler events, Element container)
        {
            _events = events;
            _container = container;
        }

        /// <summary>
        /// Shows elements.
        /// </summary>
        public void Show()
        {
            Vine.OnElementCreated += Vine_OnCreated;
            Vine.enabled = true;

            _events.AddHandler(MessageTypes.BUTTON_ACTIVATE, this);
        }

        /// <summary>
        /// Hides elements.
        /// </summary>
        public void Hide()
        {
            _events.RemoveHandler(MessageTypes.BUTTON_ACTIVATE, this);

            Vine.enabled = false;
            Vine.OnElementCreated -= Vine_OnCreated;
        }

        /// <summary>
        /// Uninitializes the controller. Show/Hide should not be called again
        /// until Initialize is called.
        /// </summary>
        public void Uninitialize()
        {
            //
        }

        /// <inheritdoc cref="IIUXEventDelegate"/>
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

        /// <summary>
        /// Called when the vine creates an element.
        /// </summary>
        /// <param name="element">The element created.</param>
        private void Vine_OnCreated(Element element)
        {
            _container.AddChild(element);
        }
    }
}