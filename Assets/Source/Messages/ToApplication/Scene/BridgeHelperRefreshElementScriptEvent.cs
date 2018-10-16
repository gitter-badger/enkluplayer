using Newtonsoft.Json;

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
        [JsonProperty("id")]
        public string Id { get; set; }
    }
}