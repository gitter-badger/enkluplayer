using LightJson;

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
        [JsonName("trellisBaseUrl")]
        public string TrellisBaseUrl;

        /// <summary>
        /// Asset base url.
        /// </summary>
        [JsonName("assetBaseUrl")]
        public string AssetBaseUrl;

        /// <summary>
        /// Bundles url.
        /// </summary>
        [JsonName("bundlesUrl")]
        public string BundlesUrl;

        /// <summary>
        /// Thumbs url.
        /// </summary>
        [JsonName("thumbsUrl")]
        public string ThumbsUrl;
    }
}