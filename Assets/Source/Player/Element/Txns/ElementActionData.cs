using CreateAR.EnkluPlayer.IUX;
using LightJson;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Grab bag of action data.
    /// </summary>
    public class ElementActionData
    {
        /// <summary>
        /// Type of action.
        /// </summary>
        [JsonName("type")]
        public string Type;

        /// <summary>
        /// Id of the element this action affects.
        /// </summary>
        [JsonName("elementId")]
        public string ElementId;
        
        /// <summary>
        /// Id of the parent node, used for create actions.
        /// </summary>
        [JsonName("parentId")]
        public string ParentId;

        /// <summary>
        /// The element structure, used for create actions.
        /// </summary>
        [JsonName("data")]
        public ElementData Element;

        /// <summary>
        /// Type of schema data to affect, used for update actions.
        /// </summary>
        [JsonName("schemaType")]
        public string SchemaType;

        /// <summary>
        /// Key for schema data to affect, used for update actions.
        /// </summary>
        [JsonName("key")]
        public string Key;

        /// <summary>
        /// Value of schema data, used for update actions.
        /// </summary>
        [JsonName("value")]
        public object Value;

        /// <summary>
        /// Useful ToString.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format(
                "[ElementActionData Type={0}, ElementId={1}, ParentId={2}, Element={3}, SchemaType={4}, Key={5}, Value={6}]",
                Type,
                ElementId,
                ParentId,
                Element,
                SchemaType,
                Key,
                Value);
        }
    }
}