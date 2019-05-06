using Newtonsoft.Json;
using Enklu.Data;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Event when actions should be directly applied.
    /// </summary>
    public class BridgeHelperActionEvent
    {
        /// <summary>
        /// Scene to apply actions.
        /// </summary>
        [JsonProperty("sceneId")]
        public string SceneId { get; set; }

        /// <summary>
        /// Actions.
        /// </summary>
        [JsonProperty("actions")]
        public ElementActionData[] Actions { get; set; }
    }
}