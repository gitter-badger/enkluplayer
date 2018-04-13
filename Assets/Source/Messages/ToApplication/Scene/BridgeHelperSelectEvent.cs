using LightJson;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Select event.
    /// </summary>
    public class BridgeHelperSelectEvent
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
    }
}