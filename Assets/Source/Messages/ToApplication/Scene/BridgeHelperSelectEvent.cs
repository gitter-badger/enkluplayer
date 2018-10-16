using Newtonsoft.Json;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Select event.
    /// </summary>
    public class BridgeHelperSelectEvent
    {
        /// <summary>
        /// Scene.
        /// </summary>
        [JsonProperty("sceneId")]
        public string SceneId { get; set; }

        /// <summary>
        /// Element.
        /// </summary>
        [JsonProperty("elementId")]
        public string ElementId { get; set; }
    }
}