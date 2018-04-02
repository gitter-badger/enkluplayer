using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.IUX;
using Jint;

namespace CreateAR.SpirePlayer
{
    public class AppElementsJsApi
    {
        private readonly IElementFactory _elementFactory;
        private readonly IElementManager _elements;
        private readonly Engine _engine;
        
        private readonly Dictionary<string, ElementJs> _elementMap = new Dictionary<string, ElementJs>();

        public AppElementsJsApi(
            IElementFactory elementFactory,
            IElementManager elements,
            Engine engine)
        {
            _elementFactory = elementFactory;
            _elements = elements;
            _engine = engine;
        }
        
        public ElementJs create(string type)
        {
            Element element;
            try
            {
                element = _elementFactory.Element(string.Format(
                    @"<?Vine><{0} id='{1}' />",
                    type,
                    Guid.NewGuid().ToString()));
            }
            catch (Exception exception)
            {
                Log.Error(this,
                    "Could not create Element : {0}.",
                    exception);
                return null;
            }
            
            element.OnDestroyed += Element_OnDestroyed;
            
            var wrapper = _elementMap[element.Id] = new ElementJs(_engine, element);

            return wrapper;
        }

        public ElementJs byId(string id)
        {
            ElementJs element;
            if (_elementMap.TryGetValue(id, out element))
            {
                return element;
            }

            // lazily create wrappers
            var el = _elements.ById(id);
            if (null == el)
            {
                return null;
            }
            
            element = new ElementJs(_engine, el);
            _elementMap[id] = element;
            
            return element;
        }
        
        private void Element_OnDestroyed(Element element)
        {
            _elementMap.Remove(element.Id);
        }
    }
    
    public class AppJsApi
    {
        public readonly AppElementsJsApi elements;
        
        public AppJsApi(AppElementsJsApi elements)
        {
            this.elements = elements;
        }
    }
}