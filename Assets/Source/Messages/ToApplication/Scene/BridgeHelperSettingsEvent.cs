using LightJson;

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
        [JsonName("category")]
        public string Category;

        /// <summary>
        /// Name of the setting.
        /// </summary>
        [JsonName("name")]
        public string Name;

        /// <summary>
        /// Setting value.
        /// </summary>
        [JsonName("value")]
        public bool Value;
    }
}