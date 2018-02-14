using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Messaging;
using UnityEngine;
using Object = UnityEngine.Object;

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
        /// List of verts.
        /// </summary>
        private readonly List<UIVertex> _vertices = new List<UIVertex>();

        /// <summary>
        /// Renders text.
        /// </summary>
        private TextRenderer _renderer;

        /// <summary>
        /// Props.
        /// </summary>
        private ElementSchemaProp<string> _propAlignment;
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
        /// Bounding rectangle in local space.
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
        /// Bounding rectangle of rendered text in world space.
        /// </summary>
        public Rectangle TextRect
        {
            get
            {
                var gen = _renderer.Text.cachedTextGenerator;
                var maxY = float.MinValue;
                var minY = float.MaxValue;
                var maxX = float.MinValue;
                var minX = float.MaxValue;

                _vertices.Clear();
                gen.GetVertices(_vertices);

                if (_vertices.Count == 0)
                {
                    maxX = maxY = minX = minY = 0;
                }
                
                for (var index = 0; index < _vertices.Count; index++)
                {
                    var pos = _vertices[index].position;

                    maxY = Mathf.Max(maxY, pos.y);
                    maxX = Mathf.Max(maxX, pos.x);

                    minY = Mathf.Min(minY, pos.y);
                    minX = Mathf.Min(minX, pos.x);
                }

                var trans = _renderer.Text.transform;
                var scale = trans.localScale;

                // must include the parent too
                var offset = trans.localPosition + _renderer.transform.localPosition;

                return new Rectangle(
                    minX * scale.x + offset.x,
                    minY * scale.y + offset.y,
                    (maxX - minX) * scale.x,
                    (maxY - minY) * scale.y);
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
        /// Sets overflow method.
        /// </summary>
        public HorizontalWrapMode Overflow
        {
            get { return _renderer.Text.horizontalOverflow; }
            set { _renderer.Text.horizontalOverflow = value; }
        }

        /// <summary>
        /// Alignment.
        /// </summary>
        public TextAlignmentType Alignment
        {
            get { return _renderer.Alignment; }
            set { _renderer.Alignment = value; }
        }
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public TextPrimitive(
            WidgetConfig config,
            IMessageRouter messages,
            ILayerManager layers,
            TweenConfig tweens,
            ColorConfig colors)
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
        protected override void LoadInternalAfterChildren()
        {
            base.LoadInternalAfterChildren();

            _renderer = Object.Instantiate(
                _config.Text,
                Vector3.zero,
                Quaternion.identity);
            _renderer.transform.SetParent(GameObject.transform, false);
            
            // load font setup.
            _propAlignment = Schema.Get<string>("alignment");
            _propAlignment.OnChanged += Alignment_OnChanged;
            UpdateAlignment(_propAlignment.Value);

            _propFont = Schema.Get<string>("font");
            _propFont.OnChanged += Font_OnChanged;
            _renderer.Font = _propFont.Value;

            _propFontSize = Schema.Get<int>("fontSize");
            _propFontSize.OnChanged += FontSize_OnChanged;
            _renderer.FontSize = _propFontSize.Value;
        }

        /// <inheritdoc cref="Element"/>
        protected override void UnloadInternalAfterChildren()
        {
            base.UnloadInternalAfterChildren();

            _propAlignment.OnChanged -= Alignment_OnChanged;
            _propFont.OnChanged -= Font_OnChanged;
            _propFontSize.OnChanged -= FontSize_OnChanged;
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
            var handle = Render.Handle("IUX.Text");
            if (null == handle)
            {
                return;
            }

            var pos = _renderer.transform.position;
            var rect = Rect;
            var textRect = TextRect;
            
            handle.Draw(ctx =>
            {
                ctx.Prism(new Bounds(
                    pos,
                    new Vector3(
                        rect.size.x,
                        rect.size.y,
                        0)));

                ctx.Color(new Color(1, 0, 0, 1));
                ctx.Prism(new Bounds(
                    new Vector3(
                        textRect.min.x + textRect.size.x / 2f,
                        textRect.min.y + textRect.size.y / 2f,
                        pos.z), 
                    new Vector3(
                        textRect.size.x,
                        textRect.size.y,
                        0)));
            });
        }

        /// <summary>
        /// Updats alignment from a string.
        /// </summary>
        /// <param name="alignment">String value.</param>
        private void UpdateAlignment(string alignment)
        {
            _renderer.Alignment = EnumExtensions.Parse<TextAlignmentType>(alignment);
        }

        /// <summary>
        /// Called when the label has been updated.
        /// </summary>
        /// <param name="prop">Alignment prop.</param>
        /// <param name="prev">Previous value.</param>
        /// <param name="next">Next value.</param>
        private void Alignment_OnChanged(
            ElementSchemaProp<string> prop,
            string prev,
            string next)
        {
            UpdateAlignment(next);
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