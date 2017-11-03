namespace CreateAR.SpirePlayer.UI
{
    public class ElementFactory : IElementFactory
    {
        public Element Element(ElementDescription description)
        {
            return Element(description.Collapsed());
        }
        
        private Element Element(ElementData data)
        {
            // create element from data
            var element = new Element();
            var schema = new ElementSchema();

            // children first
            var childData = data.Children;
            var childDataLen = childData.Length;
            var children = new Element[childDataLen];
            for (int i = 0, len = childData.Length; i < len; i++)
            {
                children[i] = Element(childData[i]);
            }

            // element
            schema.Load(data.Schema);
            element.Load(data, schema, children);

            return element;
        }
    }
}