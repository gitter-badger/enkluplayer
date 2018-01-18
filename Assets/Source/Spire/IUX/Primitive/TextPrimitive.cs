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
        /// Renders text.
        /// </summary>
        private TextRenderer _renderer;

        /// <summary>
        /// Props.
        /// </summary>
        private ElementSchemaProp<int> _propAlignment;
        private ElementSchemaProp<string> _propFont;
        private ElementSchemaProp<int> _propFontSize;

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
/*
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
        */
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
                var trans = _renderer.Text.rectTransform;
                var rect = trans.rect;
                var scale = trans.localScale;
                return new Rectangle(
                    rect.x * scale.x,
                    rect.y * scale.y,
                    rect.width * scale.x,
                    rect.height * scale.y);
            }
        }

        /// <summary>
        /// Retrieves the width of the primitive.
        /// </summary>
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

        /// <summary>
        /// Retrieves the height of the primitive.
        /// </summary>
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
            
            // load font setup.
            _propAlignment = Schema.GetOwn("alignment", AlignmentTypes.MID_CENTER);
            _propAlignment.OnChanged += Alignment_OnChanged;
            _renderer.Alignment = _propAlignment.Value;

            _propFont = Schema.Get<string>("font");
            _propFont.OnChanged += Font_OnChanged;
            _renderer.Font = _propFont.Value;

            _propFontSize = Schema.Get<int>("fontSize");
            _propFontSize.OnChanged += FontSize_OnChanged;
            _renderer.FontSize = _propFontSize.Value;
        }

        /// <inheritdoc cref="Element"/>
        protected override void UnloadInternal()
        {
            _propAlignment.OnChanged -= Alignment_OnChanged;
            _propFont.OnChanged -= Font_OnChanged;
            _propFontSize.OnChanged -= FontSize_OnChanged;

            Object.Destroy(_renderer.gameObject);

            base.UnloadInternal();
        }

        /// <inheritdoc cref="Element"/>
        protected override void LateUpdateInternal()
        {
            base.LateUpdateInternal();

            DebugDraw();
        }

        /// <summary>
        /// Draws debug lines.
        /// </summary>
        /// [Conditional("ELEMENT_DEBUGGING")]
        private void DebugDraw()
        {
            var handle = Render.Handle("IUX");
            if (null == handle)
            {
                return;
            }

            handle.Draw(ctx =>
            {
                ctx.Prism(new Bounds(
                    _renderer.Text.rectTransform.position,
                    new Vector3(
                        Width,
                        Height,
                        0)));
            });
        }

        /// <summary>
        /// Called when the label has been updated.
        /// </summary>
        /// <param name="prop">Alignment prop.</param>
        /// <param name="prev">Previous value.</param>
        /// <param name="next">Next value.</param>
        private void Alignment_OnChanged(
            ElementSchemaProp<int> prop,
            int prev,
            int next)
        {
            _renderer.Alignment = next;
        }

        /// <summary>
        /// Called when the label has been updated.
        /// </summary>
        /// <param name="prop">FontSize prop.</param>
        /// <param name="prev">Previous value.</param>
        /// <param name="next">Next value.</param>
        private void FontSize_OnChanged(
            ElementSchemaProp<int> prop,
            int prev,
            int next)
        {
            _renderer.FontSize = next;
        }

        /// <summary>
        /// Called when the label has been updated.
        /// </summary>
        /// <param name="prop">Font prop.</param>
        /// <param name="prev">Previous value.</param>
        /// <param name="next">Next value.</param>
        private void Font_OnChanged(
            ElementSchemaProp<string> prop,
            string prev,
            string next)
        {
            _renderer.Font = next;
        }
    }
}