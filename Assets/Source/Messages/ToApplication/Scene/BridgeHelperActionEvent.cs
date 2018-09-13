using LightJson;

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
        [JsonName("sceneId")]
        public string SceneId;

        /// <summary>
        /// Actions.
        /// </summary>
        [JsonName("actions")]
        public ElementActionData[] Actions;
    }
}