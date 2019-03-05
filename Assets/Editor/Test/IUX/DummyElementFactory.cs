using System;
using CreateAR.EnkluPlayer.IUX;
using Enklu.Data;
using ElementDescription = CreateAR.EnkluPlayer.IUX.ElementDescription;

namespace CreateAR.EnkluPlayer.Test.UI
{
    public class DummyElementFactory : IElementFactory
    {
        public Element Element(ElementDescription description)
        {
            return Element(description.Collapsed());
        }

        public Element Element(string vine)
        {
            throw new NotImplementedException();
        }

        private Element Element(ElementData data)
        {
            // children first
            var childData = data.Children;
            var childDataLen = childData.Length;
            var children = new Element[childDataLen];
            for (int i = 0, len = childData.Length; i < len; i++)
            {
                children[i] = Element(childData[i]);
            }

            // element
            var schema = new ElementSchema(data.Id);
            schema.Load(data.Schema);

            var element = new Element();
            element.Load(data, schema, children);

            return element;
        }
    }
}