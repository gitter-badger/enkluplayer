using System.Linq;

namespace CreateAR.SpirePlayer.IUX
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
        /// Type of element to construct.
        /// </summary>
        public int Type;
        
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
            Type = data.Type;
            Children = data.Children.ToArray();
            Schema = new ElementSchemaData(data.Schema);
        }

        /// <summary>
        /// Useful ToString override.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("[ElementData Id={0}, Type={1}, ChildCount={2}]",
                Id,
                Type,
                Children.Length);
        }
    }
}