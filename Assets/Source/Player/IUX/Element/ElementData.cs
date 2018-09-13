using System.Linq;
using LightJson;

namespace CreateAR.EnkluPlayer.IUX
{
    /// <summary>
    /// Saved data for elements.
    /// </summary>
    public class ElementData
    {
        /// <summary>
        /// Unique to this set element.
        /// </summary>
        [JsonName("id")]
        public string Id;

        /// <summary>
        /// Type of element to construct.
        /// </summary>
        [JsonName("type")]
        public int Type;

        /// <summary>
        /// Set of children.
        /// </summary>
        [JsonName("children")]
        public ElementData[] Children = new ElementData[0];

        /// <summary>
        /// Schema object.
        /// </summary>
        [JsonName("schema")]
        public ElementSchemaData Schema = new ElementSchemaData();
        
        /// <summary>
        /// Default constructor.
        /// </summary>
        public ElementData()
        {
            
        }

        /// <summary>
        /// Creates elementdata from an element.
        /// </summary>
        /// <param name="element">The element.</param>
        public ElementData(Element element)
        {
            Id = element.Id;
            Type = ElementTypes.TypeFromElement(element);
            Schema = new ElementSchemaData();

            foreach (var prop in element.Schema)
            {
                var type = prop.Type;
                if (typeof(int) == type)
                {
                    Schema.Ints[prop.Name] = ((ElementSchemaProp<int>) prop).Value;
                }
                else if (typeof(float) == type)
                {
                    Schema.Floats[prop.Name] = ((ElementSchemaProp<float>) prop).Value;
                }
                else if (typeof(bool) == type)
                {
                    Schema.Bools[prop.Name] = ((ElementSchemaProp<bool>) prop).Value;
                }
                else if (typeof(string) == type)
                {
                    Schema.Strings[prop.Name] = ((ElementSchemaProp<string>) prop).Value;
                }
                else if (typeof(Col4) == type)
                {
                    Schema.Colors[prop.Name] = ((ElementSchemaProp<Col4>) prop).Value;
                }
                else if (typeof(Vec3) == type)
                {
                    Schema.Vectors[prop.Name] = ((ElementSchemaProp<Vec3>) prop).Value;
                }
            }

            var numChildren = element.Children.Count;
            Children = new ElementData[numChildren];
            for (var i = 0; i < numChildren; i++)
            {
                Children[i] = new ElementData(element.Children[i]);
            }
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        /// <param name="data">The data to create this from.</param>
        public ElementData(ElementData data)
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