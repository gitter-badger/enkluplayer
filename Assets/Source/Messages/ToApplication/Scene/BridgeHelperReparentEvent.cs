using Newtonsoft.Json;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Event received over bridge to help reparent.
    /// </summary>
    public class BridgeHelperReparentEvent
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

        /// <summary>
        /// Parent.
        /// </summary>
        [JsonProperty("parentId")]
        public string ParentId { get; set; }
    }
}