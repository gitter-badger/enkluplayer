using System;
using CreateAR.Commons.Unity.Messaging;
using UnityEngine;

namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// Base class for widgets that are also <c>IInteractables</c>.
    /// </summary>
    public abstract class InteractableWidget : Widget, IInteractable
    {
        /// <summary>
        /// Backing vatiable for Focused property.
        /// </summary>
        private bool _focused;

        /// <summary>
        /// True iff we are registered with <c>IInteractionManager</c>.
        /// </summary>
        private bool _isInteractable;

        /// <summary>
        /// Interactions.
        /// </summary>
        private readonly IInteractionManager _interactions;

        /// <summary>
        /// Interactions.
        /// </summary>
        private readonly IMessageRouter _messages;

        /// <inheritdoc />
        public bool IsHighlighted { get; set; }

        /// <inheritdoc />
        public float Aim { get; protected set; }

        /// <inheritdoc />
        public virtual bool Focused
        {
            get
            {
                return _focused;
            }
            set
            {
                if (_focused == value)
                {
                    return;
                }

                _focused = value;

                if (!_isInteractable)
                {
                    return;
                }

                if (_focused)
                {
                    _messages.Publish(
                        MessageTypes.WIDGET_FOCUS,
                        new WidgetFocusEvent(this));
                }
                else
                {
                    _messages.Publish(
                        MessageTypes.WIDGET_UNFOCUS,
                        new WidgetUnfocusEvent(this));
                }
            }
        }

        /// <inheritdoc />
        public bool Interactable { get; protected set; }

        /// <inheritdoc />
        public int HighlightPriority { get; set; }

        /// <summary>
        /// Hides the highlight widget.
        /// </summary>
        public bool HideHighlightWidget;

        /// <summary>
        /// Shows widget if highlighted.
        /// </summary>
        public Widget ShowIfHighlightedWidget;

        /// <inheritdoc />
        public event Action<IInteractable> OnVisibilityChanged;

        /// <summary>
        /// Constructor.
        /// </summary>
        protected InteractableWidget(
            GameObject gameObject,
            WidgetConfig config,
            ILayerManager layers,
            TweenConfig tweens,
            ColorConfig colors,
            IMessageRouter messages,
            IInteractionManager interactions)
            : base(
                gameObject,
                config,
                layers,
                tweens,
                colors,
                messages)
        {
            _interactions = interactions;
            _messages = messages;

            _isVisible.OnChanged += Visible_OnChanged;
        }

        /// <inheritdoc />
        public abstract bool Raycast(Vec3 origin, Vec3 direction);

        /// <inheritdoc />
        protected override void AfterLoadChildrenInternal()
        {
            base.AfterLoadChildrenInternal();

            _interactions.Add(this);
            _isInteractable = true;
        }

        /// <inheritdoc />
        protected override void BeforeUnloadChildrenInternal()
        {
            base.BeforeUnloadChildrenInternal();

            _isInteractable = false;
            _interactions.Remove(this);
        }

        /// <inheritdoc />
        protected override void UpdateInternal()
        {
            base.UpdateInternal();

            if (!Visible)
            {
                return;
            }

            if (ShowIfHighlightedWidget == null)
            {
                return;
            }

            var isHighlighted = false;
            var highlighted = _interactions.Highlighted;
            if (highlighted != null)
            {
                if (highlighted == this)
                {
                    isHighlighted = true;
                }
                else
                {
                    var widget = highlighted as Widget;
                    if (null != widget)
                    {
                        if ( IsDescendant(widget.GameObject.transform, GameObject.transform)
                             || IsDescendant(GameObject.transform, widget.GameObject.transform))
                        {
                            isHighlighted = true;
                        }
                    }
                }
            }

            ShowIfHighlightedWidget.LocalVisible = isHighlighted && !HideHighlightWidget;
        }

        /// <summary>
        /// Called when the visibility changes.
        /// </summary>
        /// <param name="value">The visibility value.</param>
        private void Visible_OnChanged(bool value)
        {
            if (null != OnVisibilityChanged)
            {
                OnVisibilityChanged(this);
            }
        }

        /// <summary>
        /// Checks if a transform is a decendent of another.
        /// </summary>
        /// <param name="ancestor">The ancestor to start at.</param>
        /// <param name="descendant">The decendant to check.</param>
        /// <returns></returns>
        protected static bool IsDescendant(Transform ancestor, Transform descendant)
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
        }
    }
}