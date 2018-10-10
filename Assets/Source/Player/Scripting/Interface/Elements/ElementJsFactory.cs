using CreateAR.EnkluPlayer.IUX;
using Jint;

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
        public ElementJs Instance(Engine engine, Element element)
        {
            var cache = new ElementJsCache(this, engine);

            var type = element.GetType();
            if (type == typeof(ButtonWidget))
            {
                return new ButtonElementJs(_scripts, cache, engine, element);
            }
            if (type == typeof(ContentWidget))
            {
                return new ContentElementJs(_scripts, cache, engine, element);
            }

            return new ElementJs(_scripts, cache, engine, element);
        }
    }
}