using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using CreateAR.SpirePlayer.UI;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// A widget that toggles another widget on or off depending on highlight.
    /// </summary>
    public class InteractiveWidget : Widget, IInteractive
    {
        /// <summary>
        /// Dependencies
        /// </summary>
        public IInteractionManager Interactions { get; set; }

        /// <summary>
        /// True if the widget is currently focused
        /// </summary>
        private bool _isFocused;

        /// <summary>
        /// True if the widget is currently highlighted
        /// </summary>
        private ElementSchemaProp<bool> _isHighlighted;

        /// <summary>
        /// True if the widget can be interacted with
        /// </summary>
        private ElementSchemaProp<bool> _isInteractionEnabled;

        /// <summary>
        /// True if the widget can be interacted with
        /// </summary>
        private ElementSchemaProp<int> _highlightPriority;

        /// <summary>
        /// Interacble Primitive
        /// </summary>
        public IInteractivePrimitive InteractivePrimitive { get; protected set; }

        /// <summary>
        /// Highligted Accessor/Mutator
        /// </summary>
        public bool IsHighlighted
        {
            get { return _isHighlighted.Value; }
            set { _isHighlighted.Value = value; }
        }

        /// <summary>
        /// If true, can be interacted with.
        /// </summary>
        public bool IsInteractionEnabled
        {
            get { return _isInteractionEnabled.Value; }
            set { _isInteractionEnabled.Value = value; }
        }

        /// <summary>
        /// If true, can be interacted with.
        /// </summary>
        public int HighlightPriority
        {
            get { return _highlightPriority.Value; }
            set { _highlightPriority.Value = value; }
        }

        /// <summary>
        /// Returns true if interactable.
        /// </summary>
        public bool IsInteractable
        {
            get
            {
                const float FOCUSABLE_THRESHOLD = 0.99f;
                return IsVisible && Tween > FOCUSABLE_THRESHOLD
                    && IsInteractionEnabled
                    && (!Interactions.IsOnRails || IsHighlighted);
            }
        }

        /// <summary>
        /// True if the widget is focused.
        /// </summary>
        public virtual bool IsFocused
        {
            get
            {
                return _isFocused;
            }
            set
            {
                if (_isFocused != value)
                {
                    _isFocused = value;

                    if (_isFocused)
                    {
                        Messages.Publish(MessageTypes.WIDGET_FOCUS, new WidgetFocusEvent());
                    }
                    else
                    {
                        Messages.Publish(MessageTypes.WIDGET_UNFOCUS, new WidgetUnfocusEvent());
                    }
                }
            }
        }
        
        /// <summary>
        /// Dependency initialization.
        /// </summary>
        public void Initialize(
            IWidgetConfig config,
            ILayerManager layers,
            ITweenConfig tweens,
            IColorConfig colors,
            IPrimitiveFactory primitives,
            IMessageRouter messages,
            IInteractionManager interactions)
        {
            Interactions = interactions;
            Initialize(config, layers, tweens, colors, primitives, messages);
        }

        /// <summary>
        /// Initialization
        /// </summary>
        protected override void LoadInternal()
        {
            base.LoadInternal();

            _isHighlighted = Schema.Get<bool>("isHighlighted");
            _isInteractionEnabled = Schema.Get<bool>("isInteractionEnabled");
            _highlightPriority = Schema.Get<int>("highlightPriority");

            // TODO: fix this once "hasProp" is implemented
            _isInteractionEnabled.Value = true;
        }

        /// <summary>
        /// Updates visibility of ShowIfHighlightedWidget.
        /// </summary>
        protected override void UpdateInternal()
        {
            base.UpdateInternal();

            if (IsVisible)
            {
                InteractivePrimitive.SetInteractionEnabled(IsInteractable);
            }
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