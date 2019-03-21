using CreateAR.EnkluPlayer.IUX;

namespace CreateAR.EnkluPlayer.Scripting
{
    /// <summary>
    /// Basic implementation of <c>IElementJsFactory</c>.
    /// </summary>
    public class ElementJsFactory : IElementJsFactory
    {
        /// <summary>
        /// Scripts!
        /// </summary>
        private readonly IScriptManager _scripts;
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public ElementJsFactory(IScriptManager scripts)
        {
            _scripts = scripts;
        }

        /// <inheritdoc />
        public ElementJs Instance(IElementJsCache jsCache, Element element)
        {
            var type = element.GetType();
            if (type == typeof(ButtonWidget))
            {
                return new ButtonElementJs(_scripts, jsCache, element);
            }

            if (type == typeof(ContentWidget))
            {
                return new ContentWidgetJs(_scripts, jsCache, element);
            }

            return new ElementJs(_scripts, jsCache, element);
        }
    }
}