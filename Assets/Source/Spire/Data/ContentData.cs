using System;

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
        public bool Unique;

        /// <summary>
        /// If true, does not inherit color from parent.
        /// </summary>
        public bool PreserveColor;

        /// <summary>
        /// Tags associated with this piece of content.
        /// </summary>
        public string[] Tags;

        /// <summary>
        /// Data about the asset.
        /// </summary>
        public AssetReference Asset;

        /// <summary>
        /// Anchoring Data
        /// </summary>
        public AnchorData Anchor;
    }
}