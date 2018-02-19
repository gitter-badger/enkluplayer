using LightJson;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Request for updating an element.
    /// </summary>
    public class UpdateElementRequest : ElementRequest
    {
        /// <summary>
        /// Id of element to create.
        /// </summary>
        [JsonName("elementId")]
        public string ElementId;

        /// <summary>
        /// SchemaType of parent.
        /// </summary>
        [JsonName("schemaType")]
        public string SchemaType;

        /// <summary>
        /// Schema key.
        /// </summary>
        [JsonName("key")]
        public string Key;
    }

    /// <summary>
    /// Updates an int.
    /// </summary>
    public class UpdateElementIntRequest : UpdateElementRequest
    {
        /// <summary>
        /// Int value.
        /// </summary>
        [JsonName("value")]
        public int Value;
    }

    /// <summary>
    /// Updates a float.
    /// </summary>
    public class UpdateElementFloatRequest : UpdateElementRequest
    {
        /// <summary>
        /// Float value.
        /// </summary>
        [JsonName("value")]
        public float Value;
    }

    /// <summary>
    /// Updates a string.
    /// </summary>
    public class UpdateElementStringRequest : UpdateElementRequest
    {
        /// <summary>
        /// String value.
        /// </summary>
        [JsonName("value")]
        public string Value;
    }

    /// <summary>
    /// Updates a bool.
    /// </summary>
    public class UpdateElementBoolRequest : UpdateElementRequest
    {
        /// <summary>
        /// bool value.
        /// </summary>
        [JsonName("value")]
        public bool Value;
    }

    /// <summary>
    /// Updates a vec3.
    /// </summary>
    public class UpdateElementVec3Request : UpdateElementRequest
    {
        /// <summary>
        /// Vec3 value.
        /// </summary>
        [JsonName("value")]
        public Vec3 Value;
    }

    /// <summary>
    /// Updates a col4.
    /// </summary>
    public class UpdateElementCol4Request : UpdateElementRequest
    {
        /// <summary>
        /// Col4 value.
        /// </summary>
        [JsonName("value")]
        public Col4 Value;
    }
}