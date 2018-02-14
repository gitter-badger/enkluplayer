using System;
using CreateAR.Commons.Unity.Messaging;
using UnityEngine;

namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// Basic text rendering widget.
    /// </summary>
    public class CaptionWidget : Widget
    {
        /// <summary>
        /// Primitives!
        /// </summary>
        private readonly IPrimitiveFactory _primitives;

        /// <summary>
        /// Text rendering primitive.
        /// </summary>
        private TextPrimitive _text;

        /// <summary>
        /// Text property.
        /// </summary>
        private ElementSchemaProp<string> _labelProp;
        private ElementSchemaProp<int> _fontSizeProp;
        private ElementSchemaProp<float> _widthProp;
        private ElementSchemaProp<string> _alignmentProp;
        private ElementSchemaProp<string> _overflowProp;

        /// <summary>
        /// Constructor.
        /// </summary>
        public CaptionWidget(
            GameObject gameObject,
            WidgetConfig config,
            IPrimitiveFactory primitives,
            ILayerManager layers,
            TweenConfig tweens,
            ColorConfig colors,
            IMessageRouter messages)
            : base(
                gameObject,
                config,
                layers,
                tweens,
                colors,
                messages)
        {
            _primitives = primitives;
        }

        /// <inheritdoc cref="Element"/>
        protected override void LoadInternalAfterChildren()
        {
            base.LoadInternalAfterChildren();

            _labelProp = Schema.Get<string>("label");
            _labelProp.OnChanged += Label_OnChanged;

            _fontSizeProp = Schema.Get<int>("fontSize");
            _fontSizeProp.OnChanged += FontSize_OnChanged;

            _widthProp = Schema.Get<float>("width");
            _widthProp.OnChanged += Width_OnChanged;

            _alignmentProp = Schema.Get<string>("alignment");
            _alignmentProp.OnChanged += Alignment_OnChanged;

            _overflowProp = Schema.Get<string>("overflow");
            _overflowProp.OnChanged += Overflow_OnChanged;

            _text = _primitives.Text(Schema);
            _text.Text = _labelProp.Value;
            _text.FontSize = _fontSizeProp.Value;
            _text.Width = _widthProp.Value;
            UpdateAlignment();
            UpdateOverflow();

            AddChild(_text);
        }

        /// <inheritdoc cref="Element"/>
        protected override void UnloadInternalAfterChildren()
        {
            _labelProp.OnChanged -= Label_OnChanged;
            _fontSizeProp.OnChanged -= FontSize_OnChanged;
            _widthProp.OnChanged -= Width_OnChanged;
            _alignmentProp.OnChanged -= Alignment_OnChanged;
            _overflowProp.OnChanged -= Overflow_OnChanged;

            base.UnloadInternalAfterChildren();
        }

        /// <summary>
        /// Updates alignment from the prop.
        /// </summary>
        private void UpdateAlignment()
        {
            var value = _alignmentProp.Value;

            _text.Alignment = EnumExtensions.Parse<TextAlignmentType>(value);
        }

        /// <summary>
        /// Updates overflow from the prop.
        /// </summary>
        private void UpdateOverflow()
        {
            var value = _overflowProp.Value;

            _text.Overflow = EnumExtensions.Parse<HorizontalWrapMode>(value);
        }

        /// <summary>
        /// Called when the label changes.
        /// </summary>
        /// <param name="prop">Property.</param>
        /// <param name="prev">Previous value.</param>
        /// <param name="next">Next value.</param>
        private void Label_OnChanged(
            ElementSchemaProp<string> prop,
            string prev,
            string next)
        {
            _text.Text = next;
        }

        /// <summary>
        /// Called when the font size changes.
        /// </summary>
        /// <param name="prop">Property.</param>
        /// <param name="prev">Previous value.</param>
        /// <param name="next">Next value.</param>
        private void FontSize_OnChanged(
            ElementSchemaProp<int> prop,
            int prev,
            int next)
        {
            _text.FontSize = next;
        }

        /// <summary>
        /// Called when the width changes.
        /// </summary>
        /// <param name="prop">Property.</param>
        /// <param name="prev">Previous value.</param>
        /// <param name="next">Next value.</param>
        private void Width_OnChanged(
            ElementSchemaProp<float> prop,
            float prev,
            float next)
        {
            _text.Width = next;
        }

        /// <summary>
        /// Called when the alignment changes.
        /// </summary>
        /// <param name="prop">Property.</param>
        /// <param name="prev">Previous value.</param>
        /// <param name="next">Next value.</param>
        private void Alignment_OnChanged(
            ElementSchemaProp<string> prop,
            string prev,
            string next)
        {
            UpdateAlignment();
        }
        
        /// <summary>
        /// Called when the overflow changes.
        /// </summary>
        /// <param name="prop">Property.</param>
        /// <param name="prev">Previous value.</param>
        /// <param name="next">Next value.</param>
        private void Overflow_OnChanged(
            ElementSchemaProp<string> prop,
            string prev,
            string next)
        {
            UpdateOverflow();
        }
    }
}