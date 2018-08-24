using LightJson;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Refreshes a script.
    /// </summary>
    public class BridgeHelperRefreshElementScriptEvent
    {
        /// <summary>
        /// Id of the element to refresh.
        /// </summary>
        [JsonName("id")]
        public string Id;
    }
}