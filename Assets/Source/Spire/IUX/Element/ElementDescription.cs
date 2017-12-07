using System;

namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// Describes a single <c>Element</c>, which may have many decendants. This
    /// data structure supports templates, i.e. sets of elements that can be
    /// used many times.
    /// </summary>
    public class ElementDescription
    {
        /// <summary>
        /// Root reference.
        /// </summary>
        public ElementRef Root;

        /// <summary>
        /// Collection of all elements.
        /// </summary>
        public ElementData[] Elements = new ElementData[0];

        /// <summary>
        /// Retrieves an element's data by id.
        /// </summary>
        /// <param name="id">Unique id of the element.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Collapses the data structure into a single <c>ElementData</c>. This
        /// creates instances out of templates.
        /// </summary>
        /// <returns></returns>
        public ElementData Collapsed()
        {
            return Data(Root);
        }

        /// <summary>
        /// Creates an <c>ElementData</c> from an <c>ElementRef</c>, recursively.
        /// </summary>
        /// <param name="reference">The root reference.</param>
        /// <returns></returns>
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

            // combine ref schema
            data.Schema.Compose(reference.Schema);

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