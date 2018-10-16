using Newtonsoft.Json;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Base class for element requests.
    /// </summary>
    public class ElementRequest
    {
        /// <summary>
        /// Type.
        /// </summary>
        [JsonProperty("type")]
        public string Type;

        /// <summary>
        /// Id of element to affect.
        /// </summary>
        [JsonProperty("elementId")]
        public string ElementId;
    }
}