using System;

namespace CreateAR.Spire
{
    [Serializable]
    public class ContentData
    {
        /// <summary>
        /// Unique Identifier
        /// </summary>
        public string Id;

        /// <summary>
        /// Asset Id
        /// 
        /// TODO: Move to AssetReference guids.
        /// </summary>
        public string AssetId;

        /// <summary>
        /// Anchoring Data
        /// </summary>
        public AnchorData Anchor;

        /// <summary>
        /// If true, each request gets its own instance
        /// </summary>
        public bool Unique;

        /// <summary>
        /// Defines how long the asset will stick around while unreferenced
        /// </summary>
        public float CacheDuration = 0.0f;

        /// <summary>
        /// If true, this asset is preloaded and not destroyed upon derefrencing
        /// </summary>
        public bool Pool;

        /// <summary>
        /// If true, does not inherit color
        /// </summary>
        public bool PreserveColor;
    }
}