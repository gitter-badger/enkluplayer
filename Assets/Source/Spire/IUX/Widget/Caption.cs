namespace CreateAR.SpirePlayer.UI
{
    /// <summary>
    /// Basic text rendering widget.
    /// </summary>
    public class Caption : Widget
    {
        /// <summary>
        /// Props.
        /// </summary>
        private ElementSchemaProp<string> _propText;
        private ElementSchemaProp<int> _propFontSize;

        /// <summary>
        /// Text rendering primitive.
        /// </summary>
        private ITextPrimitive _primitive;

        /// <summary>
        /// Text rendering primitive.
        /// </summary>
        public ITextPrimitive Text { get { return _primitive; } }

        /// <summary>
        /// Initialization
        /// </summary>
        protected override void LoadInternal()
        {
            base.LoadInternal();

            _primitive = Primitives.LoadText(this);

            _propText = Schema.Get<string>("text");
            _primitive.Text = _propText.Value;

            _propFontSize = Schema.Get<int>("fontSize");
            if (_propFontSize.Value > 0)
            {
                _primitive.FontSize = _propFontSize.Value;
            }
        }

        /// <summary>
        /// Shutdown
        /// </summary>
        protected override void UnloadInternal()
        {
            if (_primitive != null)
            {
                _primitive.Unload();
                _primitive = null;
            }
        }
    }
}
