using LightJson;

namespace CreateAR.EnkluPlayer
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