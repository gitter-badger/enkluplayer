using Newtonsoft.Json;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Describes a script.
    /// </summary>
    public class ScriptData : StaticData
    {
        /// <summary>
        /// Reference to asset.
        /// </summary>
        [JsonProperty("asset")]
        public AssetReference Asset;

        /// <summary>
        /// If true, plays on its own.
        /// </summary>
        [JsonProperty("autoplay")]
        public bool AutoPlay;

        /// <summary>
        /// Tags associated with this script.
        /// </summary>
        [JsonProperty("tags")]
        public string[] Tags;
    }
}