﻿using CreateAR.SpirePlayer.IUX;
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
        /// Id of the parent node, used for create actions.
        /// </summary>
        [JsonName("parent")]
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
    }
}