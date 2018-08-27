using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.IUX;
using Jint;

namespace CreateAR.SpirePlayer.Scripting
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
            Engine engine,
            Element element)
            : base(scripts, cache, engine, element)
        {
            ((ButtonWidget) element).OnActivated += Button_OnActivated;
        }

        /// <inheritdoc />
        public override void destroy()
        {
            ((ButtonWidget) _element).OnActivated -= Button_OnActivated;

            base.destroy();
        }

        /// <summary>
        /// Called when button has been activated.
        /// </summary>
        /// <param name="buttonWidget">The button.</param>
        private void Button_OnActivated(ButtonWidget buttonWidget)
        {
            Dispatch(EVENT_ACTIVATED, _cache.Element(buttonWidget));
        }
    }
}