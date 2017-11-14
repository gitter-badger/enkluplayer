namespace CreateAR.SpirePlayer.UI
{
    /// <summary>
    /// Basic text rendering widget.
    /// </summary>
    public class Caption : Widget
    {
        /// <summary>
        /// Handles rendering from unity's perspective
        /// </summary>
        private ITextPrimitive _primitive;

        /// <summary>
        /// Initialization
        /// </summary>
        protected override void LoadInternal()
        {
            base.LoadInternal();

            _primitive = Primitives.LoadText(this);

            _primitive.Text = Schema.Get<string>("text").Value;

            var fontSize = Schema.Get<int>("fontSize").Value;
            if (fontSize > 0)
            {
                _primitive.FontSize = fontSize;
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
