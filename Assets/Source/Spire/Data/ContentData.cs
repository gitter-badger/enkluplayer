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
        /// Data about the asset.
        /// </summary>
        public AssetData Asset;

        /// <summary>
        /// Anchoring Data
        /// </summary>
        public AnchorData Anchor;

        /// <summary>
        /// If true, each request gets its own instance
        /// </summary>
        public bool Unique;

        /// <summary>
        /// If true, does not inherit color
        /// </summary>
        public bool PreserveColor;
    }
}