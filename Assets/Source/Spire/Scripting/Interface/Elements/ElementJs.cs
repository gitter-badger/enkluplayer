using CreateAR.SpirePlayer.IUX;
using Jint;

namespace CreateAR.SpirePlayer.Scripting
{
    /// <summary>
    /// Js API for an element.
    /// </summary>
    public class ElementJs : IEntityJs
    {
        /// <summary>
        /// Element we're wrapping.
        /// </summary>
        private readonly Element _element;
        
        /// <summary>
        /// Caches ElementJs instances for an engine.
        /// </summary>
        private readonly ElementJsCache _cache;

        /// <summary>
        /// The schema interface.
        /// </summary>
        public readonly ElementSchemaJsApi schema;

        /// <summary>
        /// The backing transform interface <see cref="transform"/> uses.
        /// </summary>
        private readonly IElementTransformJsApi _transform;

        /// <summary>
        /// The transform interface.
        /// </summary>
        public IElementTransformJsApi transform { get { return _transform; } }

        /// <summary>
        /// Unique id of the element.
        /// </summary>
        public string id
        {
            get { return _element.Id; }
        }

        /// <summary>
        /// Type name of the element.
        /// </summary>
        public string type
        {
            get { return _element.GetType().Name; }
        }

        /// <summary>
        /// Gets the parent of the element.
        /// </summary>
        public ElementJs parent
        {
            get
            {
                return _cache.Element(_element.Parent);
            }
        }
        
        /// <summary>
        /// Array of children.
        /// </summary>
        public ElementJs[] children
        {
            get
            {
                var children = _element.Children;
                var wrappers = new ElementJs[children.Count];
                for (int i = 0, len = children.Count; i < len; i++)
                {
                    wrappers[i] = _cache.Element(children[i]);
                }

                return wrappers;
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public ElementJs(Engine engine, ElementJsCache cache, Element element)
        {
            _element = element;
            _cache = cache;
            
            schema = new ElementSchemaJsApi(engine, _element.Schema);
            _transform = new ElementTransformJsApi(_element);
        }

        /// <summary>
        /// Returns whether another element is a direct or indirect parent of this element.
        /// </summary>
        /// <param name="parent">Potential upstream element to check</param>
        /// <returns></returns>
        public bool isChildOf(ElementJs parent)
        {
            return _element.IsChildOf(parent._element);
        }
        
        /// <summary>
        /// Adds a child.
        /// </summary>
        /// <param name="element">The element to add as a child.</param>
        public void addChild(ElementJs element)
        {
            _element.AddChild(element._element);
        }

        /// <summary>
        /// Removes a child.
        /// </summary>
        /// <param name="element">The child to remove.</param>
        public void removeChild(ElementJs element)
        {
            _element.RemoveChild(element._element);
        }

        /// <summary>
        /// Destroys the element.
        /// </summary>
        public void destroy()
        {
            _element.Destroy();
        }

        /// <summary>
        /// Tests whether two ElementJs instances are equivalent, relative to their Elements
        /// </summary>
        /// <param name="this"></param>
        /// <param name="element"></param>
        /// <returns></returns>
        public static bool operator ==(ElementJs @this, object element)
        {
            if (((object)@this == null) && (element == null)) return true;
            if (element == null) return false;

            ElementJs elementJs = element as ElementJs;
            return @this._element == elementJs._element;
        }

        /// <summary>
        /// Tests whether two ElementJs instances are inequivalent, relative to their Elements
        /// </summary>
        /// <param name="this"></param>
        /// <param name="element"></param>
        /// <returns></returns>
        public static bool operator !=(ElementJs @this, object element)
        {
            if (((object)@this == null) && (element == null)) return false;
            if (element == null) return true;

            ElementJs elementJs = element as ElementJs;
            return @this._element != elementJs._element;
        }

        /// <summary>
        /// Tests whether an object equals this ElementJs instance.
        /// </summary>
        /// <param name="this"></param>
        /// <param name="element"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            ElementJs elementJs = obj as ElementJs;
            return elementJs != null && _element == elementJs._element;
        }
    }
}