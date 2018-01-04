using CreateAR.Commons.Unity.Messaging;
using UnityEngine;

namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// Renders text.
    /// </summary>
    public class TextPrimitive : Widget
    {
        /// <summary>
        /// Configuration.
        /// </summary>
        private readonly WidgetConfig _config;

        /// <summary>
        /// Dependencies.
        /// </summary>
        private readonly IInteractableManager _interactables;
        private readonly IInteractionManager _interaction;
        private readonly IIntentionManager _intention;
        
        /// <summary>
        /// Renders text.
        /// </summary>
        private TextRenderer _renderer;

        /// <summary>
        /// Bounds.
        /// </summary>
        private Rectangle _rect;

        /// <summary>
        /// Text getter/setter.
        /// </summary>
        public string Text
        {
            get
            {
                return _renderer.Text.text;
            }
            set
            {
                _renderer.Text.text = value;
            }
        }

        /// <summary>
        /// FontSize getter/setter.
        /// </summary>
        public int FontSize
        {
            get
            {
                return _renderer.Text.fontSize;
            }
            set
            {
                _renderer.Text.fontSize = value;
            }
        }

        /// <summary>
        /// TODO: All UI manipulation should be in 3D, Vec2 methods should be removed.
        /// TODO: This is a local Position accessor/mutator, and should be named as such.
        /// Position getter/setter.
        /// </summary>
        public Vec2 Position
        {
            get
            {
                var local = _renderer.Text.rectTransform.localPosition;

                return new Vec2(local.x, local.y);
            }
            set
            {
                var scale = _renderer.Text.rectTransform.localScale;

                _renderer.Text.rectTransform.localPosition = new Vector3(
                    scale.x * value.x,
                    scale.y * (value.y - _renderer.Text.font.lineHeight / 2f), 
                    _renderer.Text.rectTransform.localPosition.z);
            }
        }

        /// <summary>
        /// Position getter/setter.
        /// </summary>
        public Vector3 LocalPosition
        {
            get { return _renderer.transform.localPosition; }
            set { _renderer.transform.localPosition = value; }
        }

        /// <summary>
        /// Position getter/setter.
        /// </summary>
        public Vector3 Forward
        {
            get { return _renderer.transform.forward; }
        }

        /// <summary>
        /// Bounding rectangle.
        /// </summary>
        public Rectangle Rect
        {
            get
            {
                return _renderer.Text.rectTransform.rect.ToRectangle();
            }
        }

        public float Width
        {
            get { return Rect.size.x; }
            set
            {
                _renderer.Text.rectTransform.SetSizeWithCurrentAnchors(
                    RectTransform.Axis.Horizontal,
                    value);
            }
        }

        public float Height
        {
            get { return Rect.size.y; }
            set
            {
                _renderer.Text.rectTransform.SetSizeWithCurrentAnchors(
                    RectTransform.Axis.Vertical,
                    value);
            }
        }
        
        /// <summary>
        /// Local scale accessor/mutator
        /// </summary>
        public Vector3 LocalScale
        {
            get { return _renderer.transform.localScale; }
            set { _renderer.transform.localScale = value; }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public TextPrimitive(
            WidgetConfig config,
            IInteractableManager interactables,
            IInteractionManager interaction,
            IIntentionManager intention,
            IMessageRouter messages,
            ILayerManager layers,
            ITweenConfig tweens,
            IColorConfig colors)
            : base(
                new GameObject("Text"),
                config,
                layers,
                tweens,
                colors,
                messages)
        {
            _config = config;
            _interactables = interactables;
            _interaction = interaction;
            _intention = intention;
        }

        /// <inheritdoc cref="Element"/>
        protected override void LoadInternal()
        {
            base.LoadInternal();

            _renderer = Object.Instantiate(
                _config.Text,
                Vector3.zero,
                Quaternion.identity);
            _renderer.transform.SetParent(GameObject.transform, false);
            _renderer.Initialize(this, _config, Layers, Tweens, Colors, Messages, _intention, _interaction, _interactables);

            // TODO: load Font setup from here.
        }

        /// <inheritdoc cref="Element"/>
        protected override void UnloadInternal()
        {
            Object.Destroy(_renderer.gameObject);

            base.UnloadInternal();
        }
    }
}