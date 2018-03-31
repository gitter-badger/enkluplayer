using System.Collections.Generic;
using System.Linq;
using CreateAR.SpirePlayer.IUX;
using Jint;

namespace CreateAR.SpirePlayer
{
    public class ElementJs
    {
        private readonly Element _element;

        public readonly ElementSchemaJsApi props;
        public readonly ElementTransformJsApi transform;

        public ElementJs(Engine engine, Element element)
        {
            _element = element;
            
            props = new ElementSchemaJsApi(engine, _element.Schema);
            transform = new ElementTransformJsApi(_element);
        }

        public Element[] getChildren()
        {
            return _element.Children.ToArray();
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

        public Element findOne(string query)
        {
            return _element.FindOne<Element>(query);
        }

        public Element[] find(string query)
        {
            var list = new List<Element>();
            _element.Find(query, list);

            return list.ToArray();
        }
    }
}