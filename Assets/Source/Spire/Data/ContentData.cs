using System;

namespace CreateAR.Spire
{
    /// <summary>
    /// Associates AssetData, AnchorData, and some data about the Content.
    /// </summary>
    [Serializable]
    public class ContentData
    {
        /// <summary>
        /// Unique identifier.
        /// </summary>
        public string Id;

        /// <summary>
        /// Readable name.
        /// </summary>
        public string Name;

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
        /// Data about the asset.
        /// </summary>
        public AssetData Asset;

        /// <summary>
        /// Anchoring Data
        /// </summary>
        public AnchorData Anchor;
    }
}