namespace CreateAR.SpirePlayer.Test
{
    public interface IElementFactory
    {
        Element Element(ElementDescription description);
    }

    public class ElementFactory : IElementFactory
    {
        public Element Element(ElementDescription description)
        {
            return Element(
                description.Root,
                description);
        }

        private Element Element(
            ElementRef reference,
            ElementDescription description)
        {
            var id = reference.Id;
            var data = description.ById(id);

            var element = new Element();
            element.Load(data);

            return element;
        }
    }

    public class ElementDescription
    {
        public ElementRef Root;
        public ElementData[] Elements;

        public ElementData ById(string id)
        {
            var elements = Elements;
            for (int i = 0, len = elements.Length; i < len; i++)
            {
                var element = elements[i];
                if (element.Id == id)
                {
                    return element;
                }
            }

            return null;
        }
    }

    public class ElementRef
    {
        public string Id;
        public ElementRef[] Children;
    }

    public class ElementData
    {
        public string Id;
        public ElementData[] Children;
    }

    public class Element
    {
        public string Id { get; private set; }

        public void Load(ElementData data)
        {
            Id = data.Id;
        }

        public void Unload()
        {
            Id = string.Empty;
        }
    }
}