using CreateAR.EnkluPlayer.IUX;

namespace CreateAR.EnkluPlayer.Scripting
{
    /// <summary>
    /// ElementJs for buttons.
    /// </summary>
    public class ButtonElementJs : ElementJs
    {
        /// <summary>
        /// Name of event.
        /// </summary>
        public const string EVENT_ACTIVATED = "activated";

        /// <summary>
        /// Constructor.
        /// </summary>
        public ButtonElementJs(
            IScriptManager scripts,
            IElementJsCache cache,
            Element element)
            : base(scripts, cache, element)
        {
            ((ButtonWidget) element).OnActivated += Button_OnActivated;
        }

        /// <inheritdoc />
        public override void Cleanup()
        {
            base.Cleanup();

            ((ButtonWidget) _element).OnActivated -= Button_OnActivated;
        }

        /// <summary>
        /// Called when button has been activated.
        /// </summary>
        /// <param name="buttonWidget">The button.</param>
        private void Button_OnActivated(ButtonWidget buttonWidget)
        {
            dispatch(EVENT_ACTIVATED, _cache.Element(buttonWidget));
        }
    }
}