using CreateAR.SpirePlayer.IUX;
using Jint;

namespace CreateAR.SpirePlayer
{
    public class ElementJs
    {
        private readonly Element _element;
        private readonly ElementJsCache _cache;

        public readonly ElementSchemaJsApi props;
        public readonly ElementTransformJsApi transform;

        public string id
        {
            get { return _element.Id; }
        }

        public string type
        {
            get { return _element.GetType().Name; }
        }
        
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

        public ElementJs(Engine engine, ElementJsCache cache, Element element)
        {
            _element = element;
            _cache = cache;
            
            props = new ElementSchemaJsApi(engine, _element.Schema);
            transform = new ElementTransformJsApi(_element);
        }
        
        public void addChild(ElementJs element)
        {
            _element.AddChild(element._element);
        }

        public void removeChild(ElementJs element)
        {
            _element.RemoveChild(element._element);
        }

        public void destroy()
        {
            _element.Destroy();
        }
    }
}