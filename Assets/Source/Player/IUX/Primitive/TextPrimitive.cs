using System;
using System.Diagnostics;
using CreateAR.Commons.Unity.Logging;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace CreateAR.EnkluPlayer.IUX
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
        /// Material Manager.
        /// </summary>
        private readonly IMaterialManager _materialManager;
        
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
        private ElementSchemaProp<string> _propDisplay;

        /// <summary>
        /// Tracks how textrect changes over frames.
        /// </summary>
        private Rectangle _bakedTextRect;

        /// <summary>
        /// True when the text has been updated.
        /// </summary>
        private bool _isDirty = true;
        
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

                _isDirty = true;
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
            get
            {
                var trans = _renderer.Text.transform;

                return trans.localPosition / trans.lossyScale.x;
            }
            set
            {
                var trans = _renderer.Text.transform;

                trans.localPosition = value * trans.lossyScale.x;
            }
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
                var component = _renderer.Text;
                var rectTransform = component.rectTransform;

                LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);

                var width = LayoutUtility.GetPreferredWidth(rectTransform);
                var height = LayoutUtility.GetPreferredHeight(rectTransform);

                var minX = rectTransform.rect.x;
                var maxX = width;
                var minY = rectTransform.rect.y;
                var maxY = height;
                
                return new Rectangle(
                    minX,
                    minY,
                    maxX - minX,
                    maxY - minY);
            }
        }
        
        /// <summary>
        /// Bounding rectangle of rendered text in XY world space.
        /// </summary>
        public Rectangle WorldRect
        {
            get
            {
                var trans = _renderer.Text.transform;
                var scale = trans.localScale;

                var rectTransform = _renderer.Text.rectTransform;
                var offset = rectTransform.position;

                var rect = Rect;

                return new Rectangle(
                    rect.min.x * scale.x + offset.x,
                    rect.min.y * scale.y + offset.y,
                    rect.size.x * scale.x,
                    rect.size.y * scale.y);
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
        /// Sets horizontal overflow method.
        /// </summary>
        public HorizontalWrapMode HorizontalOverflow
        {
            get { return _renderer.Text.horizontalOverflow; }
            set { _renderer.Text.horizontalOverflow = value; }
        }

        /// <summary>
        /// Sets vertical overflow method.
        /// </summary>
        public VerticalWrapMode VerticalOverflow
        {
            get { return _renderer.Text.verticalOverflow; }
            set { _renderer.Text.verticalOverflow = value; }
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
        /// Line spacing.
        /// </summary>
        public float LineSpacing
        {
            get { return _renderer.LineSpacing; }
            set { _renderer.LineSpacing = value; }
        }

        /// <summary>
        /// Alpha.
        /// </summary>
        public float Alpha
        {
            get
            {
                return _renderer.Text.color.a;
            }
            set
            {
                var currentColor = _renderer.Text.color;
                _renderer.Text.color = new Color(
                    currentColor.r,
                    currentColor.g,
                    currentColor.b,
                    value);
            }
        }

        /// <summary>
        /// Called when TextRect is updated.
        /// </summary>
        public event Action<TextPrimitive> OnTextRectUpdated; 
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public TextPrimitive(
            WidgetConfig config,
            ILayerManager layers,
            TweenConfig tweens,
            ColorConfig colors,
            IMaterialManager materialManager)
            : base(
                new GameObject("Text"),
                layers,
                tweens,
                colors)
        {
            _config = config;
            _materialManager = materialManager;
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

            _propDisplay = Schema.Get<string>("display");
            _propDisplay.OnChanged += Display_OnChanged;
            var displayType = _propDisplay.Value;
            if (!string.IsNullOrEmpty(displayType))
            {
                Display_OnChanged(_propDisplay, null, _propDisplay.Value);
            }
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

            if (_isDirty)
            {
                _isDirty = false;

                if (null != OnTextRectUpdated)
                {
                    OnTextRectUpdated(this);
                }
            }

            DebugDraw();
        }

        /// <summary>
        /// Draws debug lines.
        /// </summary>
        [Conditional("ELEMENT_DEBUGGING")]
        private void DebugDraw()
        {
            var handle = Render.Handle("IUX.Text");
            if (null == handle)
            {
                return;
            }

            var pos = _renderer.transform.position;
            var rot = _renderer.transform.rotation;
            var rect = WorldRect;
            
            handle.Draw(ctx =>
            {
                ctx.Translate(pos);
                ctx.Rotate(rot);
                ctx.Prism(rect.size.x, rect.size.y, 0);
            });
        }

        /// <summary>
        /// Updats alignment from a string.
        /// </summary>
        /// <param name="alignment">String value.</param>
        private void UpdateAlignment(string alignment)
        {
            var alignmentEnum = EnumExtensions.Parse(
                alignment,
                _renderer.Alignment);
            // TODO: FIX this.
            if (alignmentEnum != _renderer.Alignment)
            {
                _renderer.Alignment = alignmentEnum;
            }

            _isDirty = true;
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

            _isDirty = true;
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

            _isDirty = true;
        }
        
        /// <summary>
        /// Called when the display type has been updated.
        /// </summary>
        /// <param name="prop">Display prop.</param>
        /// <param name="prev">Previous value.</param>
        /// <param name="next">Next value.</param>
        private void Display_OnChanged(
            ElementSchemaProp<string> prop,
            string prev,
            string next)
        {
            var mat = _materialManager.Material(this, "Text" + next);
            
            if (mat == null)
            {
                Log.Error(this, "No material found for " + _propDisplay.Value);
            }
            else
            {
                _renderer.Text.material = mat;
            }
        }
    }
}