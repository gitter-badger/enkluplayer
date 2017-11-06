using CreateAR.Commons.Unity.Logging;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// A widget that toggles another widget on or off depending on highlight.
    /// </summary>
    public class InteractableWidget : Widget, IFocusable
    {
        /// <summary>
        /// True if the widget is currently focused
        /// </summary>
        private bool _isFocused;

        /// <summary>
        /// True if the interactable can be interacted with
        /// </summary>
        private bool _isInteractionEnabled = true;

        /// <summary>
        /// Interaction Locked to a Specific Widget.
        /// </summary>
        public static bool OnRails = false;

        /// <summary>
        /// If true, the widget starts highlighted.
        /// </summary>
        [Header("Interaction")]
        public bool IsHighlighted = false;
        
        /// <summary>
        /// For gaining focus.
        /// </summary>
        public BoxCollider _FocusCollider;

        /// <summary>
        /// For losing focus.
        /// </summary>
        public BoxCollider _BufferCollider;

        /// <summary>
        /// If true, auto generates the buffer collider
        /// </summary>
        public bool AutoGenBufferCollider;

        /// <summary>
        /// Shows/Hides w/ Focus
        /// </summary>
        public Widget ShowIfFocusedWidget;

        /// <summary>
        /// Activates when the button is focused
        /// </summary>
        public GameObject EnableIfFocusedGameObject;

        /// <summary>
        /// Shows if highlighted.
        /// </summary>
        public Widget ShowIfHighlightedWidget;

        /// <summary>
        /// The collider used for gaining focus
        /// </summary>
        public Collider FocusCollider
        {
            get { return _FocusCollider; }
        }

        /// <summary>
        /// The collider user for losing focus
        /// </summary>
        public Collider UnfocusCollider
        {
            get { return _BufferCollider; }
        }

        /// <summary>
        /// If true, is locked and cannot be interacted with.
        /// TODO: Replace with a "locking-ref-count"
        /// </summary>
        public bool IsInteractionEnabled
        {
            get { return _isInteractionEnabled; }
            set { _isInteractionEnabled = value; }
        }

        /// <summary>
        /// Returns true if interactable.
        /// </summary>
        public bool IsInteractable
        {
            get
            {
                return
                    IsInteractionEnabled
                    && (!OnRails || IsHighlighted);
            }
        }

        /// <summary>
        /// Returns true if can be focused.
        /// </summary>
        public bool IsFocusable
        {
            get
            {
                const float FOCUSABLE_THRESHOLD = 0.99f;
                return IsInteractable
                       && IsVisible
                       && Tween > FOCUSABLE_THRESHOLD;
            }
        }

        /// <summary>
        /// Returns the radius of the widget.
        /// </summary>
        public virtual float Radius
        {
            get
            {
                var radius = 1f;
                if (null != FocusCollider)
                {
                    var size = _FocusCollider.size;
                    var scale = FocusCollider.transform.lossyScale;
                    var scaledSize = new Vector3(
                        size.x * scale.x,
                        size.y * scale.y,
                        size.z * scale.z);
                    radius = (scaledSize.x + scaledSize.y + scaledSize.z) / 3f;
                }

                return radius;
            }
        }

        /// <summary>
        /// True if the widget is focused
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

                    if (ShowIfFocusedWidget != null)
                    {
                        ShowIfFocusedWidget.LocalVisible = _isFocused;
                    }
                }
            }
        }

        /// <summary>
        /// Initializes the InteractableWidget.
        /// </summary>
        /// <param name="schema"></param>
        public virtual void SetSchema(InteractableSchema schema)
        {
            IsHighlighted |= schema.Highlight;
            HighlightPriority = schema.HighlightPriority;
        }

        /// <summary>
        /// Updates visibility of ShowIfHighlightedWidget.
        /// </summary>
        protected override void Update()
        {
            base.Update();

            if (!IsVisible)
            {
                return;
            }

            UpdateColliders();
            UpdateFocus();
            UpdateHighlight();
        }
        
        /// <summary>
        /// Updates the colliders
        /// </summary>
        private void UpdateColliders()
        {
            if (FocusCollider == null)
            {
                return;
            }

            if (AutoGenBufferCollider
             && _BufferCollider == null)
            {
                GenerateBufferCollider();
            }

            FocusCollider.enabled = IsFocusable;

            if (UnfocusCollider != null)
            {
                UnfocusCollider.enabled = FocusCollider.enabled;
            }
        }

        /// <summary>
        /// Generate buffer collider
        /// </summary>
        private void GenerateBufferCollider()
        {
            if (FocusCollider == null)
            {
                Log.Error(this, "Missing FocusCollider for AutoGenerateBufferCollider!");
                return;
            }

            if (_BufferCollider == null)
            {
                _BufferCollider = gameObject.AddComponent<BoxCollider>();
            }

            _BufferCollider.size = _FocusCollider.size * Config.AutoGeneratedWidgetBufferColliderSize;

            if (FocusCollider.transform != transform)
            {
                var size = _BufferCollider.size;
                size.x *= FocusCollider.transform.localScale.x;
                size.y *= FocusCollider.transform.localScale.y;
                size.z *= FocusCollider.transform.localScale.z;
                _BufferCollider.size = size;
            }
        }

        /// <summary>
        /// Updates the focus on the widget.
        /// </summary>
        private void UpdateFocus()
        {
            if (ShowIfFocusedWidget != null)
            {
                ShowIfFocusedWidget.LocalVisible = IsFocused;
            }

            if (EnableIfFocusedGameObject != null)
            {
                EnableIfFocusedGameObject.SetActive(IsFocused);
            }
        }

        /// <summary>
        /// Updates the highlight widget.
        /// </summary>
        private void UpdateHighlight()
        {
            if (ShowIfHighlightedWidget != null)
            {
                var isHighlighted = false;
                var highlightWidget = Elements.Highlighted;
                if (highlightWidget != null)
                {
                    if (this == (InteractableWidget)highlightWidget)
                    {
                        if (IsDescendant(highlightWidget.Transform, transform)
                            || IsDescendant(transform, highlightWidget.Transform))
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
        }
    }
}