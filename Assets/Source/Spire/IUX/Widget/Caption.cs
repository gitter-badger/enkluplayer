using CreateAR.Commons.Unity.Messaging;

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
        /// Font size.
        /// </summary>
        private ElementSchemaProp<int> _fontSize;

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
        {
            _primitives = primitives;

            Initialize(config, layers, tweens, colors, messages);
        }

        /// <inheritdoc cref="Element"/>
        protected override void LoadInternal()
        {
            base.LoadInternal();

            _text = Schema.Get<string>("text");
            _text.OnChanged += Text_OnChange;

            _fontSize = Schema.Get<int>("fontSize");
            _fontSize.OnChanged += FontSize_OnChange;

            _primitive = _primitives.Text();
            _primitive.Parent = this;
            _primitive.Text = _text.Value;
            _primitive.FontSize = _fontSize.Value;
        }

        /// <inheritdoc cref="Element"/>
        protected override void UnloadInternal()
        {
            _text.OnChanged -= Text_OnChange;
            _text = null;

            _fontSize.OnChanged -= FontSize_OnChange;
            _fontSize = null;
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

        /// <summary>
        /// Called when the font size changes.
        /// </summary>
        /// <param name="prop">Propery.</param>
        /// <param name="prev">Previous value.</param>
        /// <param name="next">Next value.</param>
        private void FontSize_OnChange(
            ElementSchemaProp<int> prop,
            int prev,
            int next)
        {
            _primitive.FontSize = next;
        }
    }
}
