using System;
using Newtonsoft.Json;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Associates AssetData, AnchorData, and some data about the Content.
    /// </summary>
    [Serializable]
    public class ContentData : StaticData
    {
        /// <summary>
        /// If true, each request from the IContentManager will create a new
        /// <c>Content</c> instance.
        /// </summary>
        [JsonProperty("unique")]
        public bool Unique;

        /// <summary>
        /// If true, does not inherit color from parent.
        /// </summary>
        [JsonProperty("preserveColor")]
        public bool PreserveColor;

        /// <summary>
        /// Tags associated with this piece of content.
        /// </summary>
        [JsonProperty("tags")]
        public string Tags;

        /// <summary>
        /// Data about the asset.
        /// </summary>
        [JsonProperty("asset")]
        public AssetReference Asset;

        /// <summary>
        /// Anchoring Data
        /// </summary>
        [JsonProperty("anchor")]
        public AnchorData Anchor;

        /// <summary>
        /// Useful ToString.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("[ContentData Id={0} Asset={1}]",
                Id,
                Asset);
        }
    }
}