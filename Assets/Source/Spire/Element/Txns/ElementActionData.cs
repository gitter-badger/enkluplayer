using LightJson;

namespace CreateAR.SpirePlayer
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
        /// Element type, used for create actions.
        /// </summary>
        [JsonName("elementType")]
        public int ElementType;

        /// <summary>
        /// Id of the parent node, used for create actions.
        /// </summary>
        [JsonName("parentId")]
        public string ParentId;

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
        /// Serialized valus of schema data, used for update actions.
        /// </summary>
        [JsonName("value")]
        public string Value;
    }
}