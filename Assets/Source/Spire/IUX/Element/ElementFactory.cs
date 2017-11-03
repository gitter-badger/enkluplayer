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

            // children first
            var childData = data.Children;
            var childDataLen = childData.Length;
            var children = new Element[childDataLen];
            for (int i = 0, len = childData.Length; i < len; i++)
            {
                children[i] = Element(childData[i]);
            }

            // parent
            element.Load(data, children);

            return element;
        }
    }
}