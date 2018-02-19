using LightJson;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Base class for element requests.
    /// </summary>
    public class ElementRequest
    {
        /// <summary>
        /// Type.
        /// </summary>
        [JsonName("type")]
        public string Type;

        /// <summary>
        /// Id of element to affect.
        /// </summary>
        [JsonName("elementId")]
        public string ElementId;
    }
}