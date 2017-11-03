using System;

namespace CreateAR.SpirePlayer.UI
{
    public class ElementDescription
    {
        public ElementRef Root;
        public ElementData[] Elements = new ElementData[0];
        
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

        public ElementData Collapsed()
        {
            return Data(Root);
        }
        
        private ElementData Data(ElementRef reference)
        {
            var source = ById(reference.Id);
            if (null == source)
            {
                throw new Exception(string.Format(
                    "{0} has no associated ElementData.",
                    reference));
            }

            // create new data with copy constructor
            var data = new ElementData(source);

            // add ref children
            var refChildrenRefs = reference.Children;
            var refChildrenData = new ElementData[refChildrenRefs.Length];
            for (int i = 0, len = refChildrenRefs.Length; i < len; i++)
            {
                refChildrenData[i] = Data(refChildrenRefs[i]);
            }

            // combine child lists
            if (refChildrenRefs.Length > 0)
            {
                var totalChildren = refChildrenRefs.Length + data.Children.Length;
                var children = new ElementData[totalChildren];

                Array.Copy(data.Children, 0, children, 0, data.Children.Length);
                Array.Copy(refChildrenData, 0, children, data.Children.Length, refChildrenData.Length);

                data.Children = children;
            }

            return data;
        }
    }
}