using System;
using System.Collections.Generic;
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
                element = _elementFactory.Element(string.Format(@"<?Vine><{0} />", type));
            }
            catch
            {
                return null;
            }
            
            element.OnDestroyed += Element_OnDestroyed;
            
            return new ElementJs(_engine, element);
        }

        private void Element_OnDestroyed(Element element)
        {
            _elementMap.Remove(element.Id);
        }

        public ElementJs byId(string id)
        {
            ElementJs element;
            if (_elementMap.TryGetValue(id, out element))
            {
                return element;
            }

            return null;
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