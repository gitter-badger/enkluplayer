using CreateAR.Commons.Unity.Messaging;
using UnityEngine;

namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// Basic text rendering widget.
    /// </summary>
    public class Caption : Widget
    {
        /// <summary>
        /// Primitives!
        /// </summary>
        private readonly IPrimitiveFactory _primitives;

        /// <summary>
        /// Text rendering primitive.
        /// </summary>
        private TextPrimitive _primitive;

        /// <summary>
        /// Text property.
        /// </summary>
        private ElementSchemaProp<string> _text;

        /// <summary>
        /// Constructor.
        /// </summary>
        public Caption(
            WidgetConfig config,
            IPrimitiveFactory primitives,
            ILayerManager layers,
            ITweenConfig tweens,
            IColorConfig colors,
            IMessageRouter messages)
            : base(
                new GameObject("Caption"),
                config,
                layers,
                tweens,
                colors,
                messages)
        {
            _primitives = primitives;
        }

        /// <inheritdoc cref="Element"/>
        protected override void AfterLoadChildrenInternal()
        {
            base.AfterLoadChildrenInternal();

            _text = Schema.Get<string>("text");
            _text.OnChanged += Text_OnChange;

            _primitive = _primitives.Text(Schema);
            _primitive.Text = _text.Value;
            AddChild(_primitive);
        }

        /// <inheritdoc cref="Element"/>
        protected override void AfterUnloadChildrenInternal()
        {
            _text.OnChanged -= Text_OnChange;
            _text = null;

            base.AfterUnloadChildrenInternal();
        }

        /// <summary>
        /// Called when the text changes.
        /// </summary>
        /// <param name="prop">Propery.</param>
        /// <param name="prev">Previous value.</param>
        /// <param name="next">Next value.</param>
        private void Text_OnChange(
            ElementSchemaProp<string> prop,
            string prev,
            string next)
        {
            _primitive.Text = next;
        }
    }
}
