using System.Collections.Generic;
using CreateAR.SpirePlayer.IUX;
using Jint;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Js API for an element.
    /// </summary>
    public class ElementJs
    {
        /// <summary>
        /// Runs scripts.
        /// </summary>
        private readonly IScriptManager _scripts;

        /// <summary>
        /// Caches ElementJs instances for an engine.
        /// </summary>
        private readonly IElementJsCache _cache;

        /// <summary>
        /// Element we're wrapping.
        /// </summary>
        private readonly Element _element;

        /// <summary>
        /// Scratch list for find.
        /// </summary>
        private readonly List<Element> _findScratch = new List<Element>();

        /// <summary>
        /// The schema interface.
        /// </summary>
        public readonly ElementSchemaJsApi schema;
        
        /// <summary>
        /// The transform interface.
        /// </summary>
        public readonly ElementTransformJsApi transform;

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
        /// Array of children.
        /// </summary>
        public ElementJs[] children
        {
            get
            {
                var all = _element.Children;

                var wrappers = new ElementJs[all.Count];
                for (int i = 0, len = all.Count; i < len; i++)
                {
                    wrappers[i] = _cache.Element(all[i]);
                }

                return wrappers;
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public ElementJs(
            IScriptManager scripts,
            IElementJsCache cache, 
            Engine engine,
            Element element)
        {
            _scripts = scripts;
            _element = element;
            _cache = cache;
            
            schema = new ElementSchemaJsApi(engine, _element.Schema);
            transform = new ElementTransformJsApi(_element);
        }
        
        /// <summary>
        /// Adds a child.
        /// </summary>
        /// <param name="element">The element to add as a child.</param>
        public void addChild(ElementJs element)
        {
            if (null == element)
            {
                return;
            }

            _element.AddChild(element._element);
        }

        /// <summary>
        /// Removes a child.
        /// </summary>
        /// <param name="element">The child to remove.</param>
        public bool removeChild(ElementJs element)
        {
            if (null == element)
            {
                return false;
            }

            return _element.RemoveChild(element._element);
        }
        
        /// <summary>
        /// Finds a single element by a query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        public ElementJs findOne(string query)
        {
            return _cache.Element(_element.FindOne<Element>(query));
        }

        /// <summary>
        /// Finds a collection of elements that match a query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        public ElementJs[] find(string query)
        {
            _findScratch.Clear();
            _element.Find(query, _findScratch);

            var results = new ElementJs[_findScratch.Count];
            for (int i = 0, len = _findScratch.Count; i < len; i++)
            {
                results[i] = _cache.Element(_findScratch[i]);
            }

            return results;
        }

        /// <summary>
        /// Destroys element.
        /// </summary>
        public void destroy()
        {
            _element.Destroy();
        }

        /// <summary>
        /// Passes a message to all attached scripts.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="parameters">The parameters.</param>
        public void send(string name, params object[] parameters)
        {
            _scripts.Send(_element.Id, name, parameters);
        }
    }
}