using Newtonsoft.Json;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Event pushed to change environment.
    /// </summary>
    public class EnvironmentInfoEvent
    {
        /// <summary>
        /// Base URL.
        /// </summary>
        [JsonProperty("trellisBaseUrl")]
        public string TrellisBaseUrl { get; set; }

        /// <summary>
        /// Asset base url.
        /// </summary>
        [JsonProperty("assetBaseUrl")]
        public string AssetBaseUrl { get; set; }

        /// <summary>
        /// Bundles url.
        /// </summary>
        [JsonProperty("bundlesUrl")]
        public string BundlesUrl { get; set; }

        /// <summary>
        /// Thumbs url.
        /// </summary>
        [JsonProperty("thumbsUrl")]
        public string ThumbsUrl { get; set; }

        /// <summary>
        /// Scripts url.
        /// </summary>
        [JsonProperty("scriptsUrl")]
        public string ScriptsUrl { get; set; }

        /// <summary>
        /// Anchors url.
        /// </summary>
        [JsonProperty("anchorsUrl")]
        public string AnchorsUrl { get; set; }
    }
}