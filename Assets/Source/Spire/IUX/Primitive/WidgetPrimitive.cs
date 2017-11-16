using UnityEngine;

namespace CreateAR.SpirePlayer.UI
{
    /// <summary>
    /// Widget primitive.
    /// </summary>
    public class WidgetPrimitive : MonoBehaviour, IPrimitive, IWidget
    {
        /// <summary>
        /// Local Tween
        /// </summary>
        private float _tween = 1.0f;

        /// <summary>
        /// For manual parenting in the hierarchy.
        /// </summary>
        public WidgetPrimitive _parent;

        /// <summary>
        /// Parent Accessor
        /// </summary>
        public IWidget Parent { get { return _parent; } }

        /// <summary>
        /// Parent widget.
        /// </summary>
        public IWidget Widget { get; private set; }

        /// <summary>
        /// Local Color.
        /// </summary>
        public Col4 _localColor = Col4.White;

        /// <summary>
        /// LocalColor Accessor/Mutator.
        /// </summary>
        public Col4 LocalColor
        {
            get { return _localColor; }
            set { _localColor = value; }
        }

        /// <summary>
        /// Current fade of the widget.
        /// </summary>
        public float Tween
        {
            get { return _tween; }
        }

        /// <summary>
        /// Current fade of the widget.
        /// </summary>
        public Layer Layer
        {
            get { return null; }
        }

        /// <summary>
        /// Game Object Accessor
        /// </summary>
        public GameObject GameObject { get { return gameObject; } }

        /// <summary>
        /// Returns the Color of the Primitive
        /// </summary>
        public Col4 Color
        {
            get
            {
                var color = LocalColor;

                if (Widget != null)
                {
                    color *= Widget.Color;
                }

                color.a *= _tween;

                return color;
            }
        }

        /// <summary>
        /// Primitives are always visible
        /// </summary>
        public bool LocalVisible { get; set; }

        /// <summary>
        /// TODO: Refactor me
        /// </summary>
        public bool Visible
        {
            get { return LocalVisible; }
        }

        /// <summary>
        /// Uses the parent as the widget for visibility and color.
        /// </summary>
        public void Awake()
        {
            LocalVisible = true;

            if (_parent != null)
            {
                Widget = _parent;
            }
        }

        /// <summary>
        /// Loads using the specified widget.
        /// </summary>
        /// <param name="widget"></param>
        public virtual void Load(IWidget widget)
        {
            Widget = widget;

            transform.SetParent(Widget.GameObject.transform, false);
            transform.gameObject.SetActive(true);
        }

        /// <summary>
        /// Frame based update
        /// </summary>
        public void Update()
        {
            var lerpDuration = LocalVisible ? 0.25f : 0.5f;
            _tween 
                = Mathf.Lerp(
                    _tween, 
                    LocalVisible ? 1.0f : 0.0f, 
                    Time.smoothDeltaTime / lerpDuration);
        }

        /// <summary>
        /// Unloads the primitive.
        /// </summary>
        public virtual void Unload()
        {
            Destroy(this);
        }

        /// <summary>
        /// Toggles Widget Visibility
        /// </summary>
        [ContextMenu("Show")]
        public void ToggleVisibility()
        {
            Widget.LocalVisible = !Widget.LocalVisible;
        }
    }
}
