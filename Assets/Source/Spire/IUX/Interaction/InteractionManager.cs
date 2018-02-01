using CreateAR.Commons.Unity.Messaging;
using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;

namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// Manages IUX elements.
    /// </summary>
    public class InteractionManager : InjectableMonoBehaviour, IInteractionManager
    {
        /// <summary>
        /// Backing variable for property.
        /// </summary>
        private readonly List<IInteractable> _all = new List<IInteractable>();

        /// <summary>
        /// Tracks the number of visible InteractableWidgets.
        /// </summary>
        private readonly List<IInteractable> _visible = new List<IInteractable>();

        /// <summary>
        /// Collection of only the highlighted elements.
        /// </summary>
        private readonly List<IInteractable> _highlighted = new List<IInteractable>();

        /// <summary>
        /// Dependendies.
        /// </summary>
        [Inject]
        public IMessageRouter Messages { get; set; }

        /// <inheritdoc />
        public IInteractable Highlighted { get; private set; }

        /// <inheritdoc />
        public bool IsOnRails { get; private set; }
        
        /// <inheritdoc />
        public ReadOnlyCollection<IInteractable> All { get; private set; }

        /// <inheritdoc />
        public ReadOnlyCollection<IInteractable> Visible { get; private set; }

        /// <inheritdoc />
        public event Action<IInteractable> OnAdded;

        /// <inheritdoc />
        public event Action<IInteractable> OnRemoved;
        
        /// <inheritdoc />
        protected override void Awake()
        {
            base.Awake();

            All = new ReadOnlyCollection<IInteractable>(_all);
            Visible = new ReadOnlyCollection<IInteractable>(_visible);

            Messages.Subscribe(MessageTypes.BUTTON_ACTIVATE, Button_OnActivate);
        }

        /// <inheritdoc />
        public void Add(IInteractable interactable)
        {
            if (null == interactable)
            {
                throw new ArgumentException("'interactable' cannot be null.");
            }

            var removed = _all.Remove(interactable);
            _all.Add(interactable);

            if (!removed)
            {
                interactable.OnVisibilityChanged += Interactable_OnVisibilityChange;
                UpdateVisibility(interactable);

                if (null != OnAdded)
                {
                    OnAdded(interactable);
                }
            }
        }

        /// <inheritdoc />
        public void Remove(IInteractable interactable)
        {
            if (null == interactable)
            {
                throw new ArgumentException("'interactable' cannot be null.");
            }

            if (_all.Remove(interactable))
            {
                interactable.OnVisibilityChanged -= Interactable_OnVisibilityChange;
                _visible.Remove(interactable);

                if (null != OnRemoved)
                {
                    OnRemoved(interactable);
                }
            }
        }

        /// <inheritdoc />
        public void Highlight(IInteractable interactable)
        {
            if (null == interactable)
            {
                throw new ArgumentException("'interactable' cannot be null.");
            }

            if (_highlighted.Contains(interactable))
            {
                return;
            }

            _highlighted.Add(interactable);
        }

        /// <inheritdoc />
        public void Unhighlight(IInteractable interactable)
        {
            if (null == interactable)
            {
                throw new ArgumentException("'interactable' cannot be null.");
            }

            _highlighted.Remove(interactable);
        }

        /// <summary>
        /// Updates the currently highlighted element.
        /// </summary>
        private void Update()
        {
            var highestPriority = int.MinValue;
            IInteractable highlightInteractable = null;

            for (int i = 0, count = _visible.Count; i < count; ++i)
            {
                var visible = _visible[i];
                if (visible.IsHighlighted)
                {
                    // TODO: Create property that a widget can implement with "Tween > 0.1"
                    //if (visible.Tween > 0.1f)
                    {
                        if (highlightInteractable == null
                            || visible.HighlightPriority > highestPriority)
                        {
                            highlightInteractable = visible;
                            highestPriority = visible.HighlightPriority;
                        }
                    }
                }
            }

            Highlighted = highlightInteractable;
        }
        
        /// <summary>
        /// Adds or removes an interactable to the visible list.
        /// </summary>
        /// <param name="interactable"></param>
        private void UpdateVisibility(IInteractable interactable)
        {
            if (interactable.Visible)
            {
                // keep the list consistent
                if (!_visible.Contains(interactable))
                {
                    _visible.Add(interactable);
                }
            }
            else
            {
                _visible.Remove(interactable);
            }
        }
        
        private void Interactable_OnVisibilityChange(IInteractable interactable)
        {
            UpdateVisibility(interactable);
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
    }
}