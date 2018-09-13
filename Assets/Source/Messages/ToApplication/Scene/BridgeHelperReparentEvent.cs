using LightJson;

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
        [JsonName("sceneId")]
        public string SceneId;

        /// <summary>
        /// Element.
        /// </summary>
        [JsonName("elementId")]
        public string ElementId;

        /// <summary>
        /// Parent.
        /// </summary>
        [JsonName("parentId")]
        public string ParentId;
    }
}