using Newtonsoft.Json;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Event fired over bridge when settings are changed.
    /// </summary>
    public class BridgeHelperSettingsEvent
    {
        /// <summary>
        /// The category.
        /// </summary>
        [JsonProperty("category")]
        public string Category { get; set; }

        /// <summary>
        /// Name of the setting.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Setting value.
        /// </summary>
        [JsonProperty("value")]
        public bool Value { get; set; }
    }
}