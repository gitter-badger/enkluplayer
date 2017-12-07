using CreateAR.Commons.Unity.Messaging;
using System.Collections.Generic;
using System;

namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// Manages IUX elements.
    /// </summary>
    public class InteractionManager : InjectableMonoBehaviour, IInteractionManager
    {
        /// <summary>
        /// Dependendies.
        /// </summary>
        [Inject]
        public IMessageRouter Messages { get; set; }

        /// <summary>
        /// Collection of only the highlighted elements.
        /// </summary>
        private readonly List<IInteractable> _highlighted = new List<IInteractable>();

        /// <summary>
        /// Retrieves the current highlighted element.
        /// 
        /// Updated every frame.
        /// </summary>
        public IInteractable Highlighted { get; private set; }

        /// <summary>
        /// True if the interaction is locked to only highlighted objects
        /// </summary>
        public bool IsOnRails { get; private set; }

        /// <summary>
        /// Initialize
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            Messages.Subscribe(MessageTypes.BUTTON_ACTIVATE, Button_OnActivate);
        }

        /// <summary>
        /// Updates the currently highlighted element.
        /// </summary>
        private void Update()
        {
            Highlighted = null;

            var highestPriority = int.MinValue;
            for (int i = 0, count = _highlighted.Count; i < count; ++i)
            {
                var interactive = _highlighted[i];
                if (!interactive.Visible)
                {
                    continue;
                }

                if (interactive.HighlightPriority < highestPriority)
                {
                    continue;
                }

                Highlighted = interactive;
                highestPriority = interactive.HighlightPriority;
            }
        }

        /// <summary>
        /// Adds an object to highlight queue. The element with the highest
        /// HighlightPriority will be highlighted.
        /// 
        /// The Highlighted property is updated every
        /// frame, not synchronously.
        /// </summary>
        /// <param name="interactable">The element to add.</param>
        public void Highlight(IInteractable interactable)
        {
            if (_highlighted.Contains(interactable))
            {
                return;
            }

            _highlighted.Add(interactable);
        }

        /// <summary>
        /// Unhighlights an element.
        /// 
        /// The Highlighted property is updated every frame, not synchronously.
        /// </summary>
        /// <param name="interactable">The element to remove.</param>
        public void Unhighlight(IInteractable interactable)
        {
            _highlighted.Remove(interactable);
        }

        /// <summary>
        /// Invoked when a button is activated.
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        private void Button_OnActivate(object arg1, Action arg2)
        {
            IsOnRails = false;
        }

        /*/// <summary>
        /// Updates the highlight widget.
        /// </summary>
        private void UpdateHighlight()
        {
            if (ShowIfHighlightedWidget != null)
            {
                var isHighlighted = false;
                var highlightWidget = Interactions.Highlighted;
                if (highlightWidget != null)
                {
                    if (this == (InteractiveWidget)highlightWidget)
                    {
                        if (IsDescendant(highlightWidget.GameObject.transform, GameObject.transform)
                         || IsDescendant(GameObject.transform, highlightWidget.GameObject.transform))
                        {
                            isHighlighted = true;
                        }
                    }
                }

                ShowIfHighlightedWidget.LocalVisible = isHighlighted;
            }
        }

        /// <summary>
        /// Checks if there is a child/parent relationship.
        /// </summary>
        private static bool IsDescendant(Transform ancestor, Transform descendant)
        {
            if (descendant == ancestor)
            {
                return true;
            }

            if (descendant.IsChildOf(ancestor))
            {
                return true;
            }

            if (descendant.parent != null)
            {
                return IsDescendant(ancestor, descendant.parent);
            }

            return false;
        }*/
    }
}