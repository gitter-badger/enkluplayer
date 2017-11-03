using System.Linq;

namespace CreateAR.SpirePlayer.UI
{
    /// <summary>
    /// Saved data for elements.
    /// </summary>
    public class ElementData
    {
        /// <summary>
        /// Unique to this set element.
        /// </summary>
        public string Id;

        /// <summary>
        /// Set of children.
        /// </summary>
        public ElementData[] Children = new ElementData[0];

        /// <summary>
        /// Schema object.
        /// </summary>
        public ElementSchemaData Schema = new ElementSchemaData();
        
        /// <summary>
        /// Default constructor.
        /// </summary>
        public ElementData()
        {
            
        }

        /// <summary>
        /// Copy constructor only provided for internal use.
        /// </summary>
        /// <param name="data">The data to create this from.</param>
        internal ElementData(ElementData data)
        {
            Id = data.Id;
            Children = data.Children.ToArray();
            Schema = new ElementSchemaData(data.Schema);
        }

        /// <summary>
        /// Useful ToString override.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("[ElementData Id={0}, ChildCount={1}]",
                Id,
                Children.Length);
        }
    }
}