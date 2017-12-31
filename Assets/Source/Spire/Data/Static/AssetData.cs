using LightJson;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Bounds object used internally to <c>AssetData</c>.
    /// </summary>
    public class AssetStatsBounds
    {
        /// <summary>
        /// Minumum.
        /// </summary>
        [JsonName("min")]
        public Vec3 Min;

        /// <summary>
        /// Maximum.
        /// </summary>
        [JsonName("max")]
        public Vec3 Max;
    }

    /// <summary>
    /// Stats for an asset.
    /// </summary>
    public class AssetStats
    {
        /// <summary>
        /// Vert count.
        /// </summary>
        [JsonName("vertCount")]
        public int VertCount;

        /// <summary>
        /// Tri count.
        /// </summary>
        [JsonName("triCount")]
        public int TriCount;

        /// <summary>
        /// Bounds.
        /// </summary>
        [JsonName("bounds")]
        public AssetStatsBounds Bounds;
    }

    /// <summary>
    /// Data for an asset.
    /// 
    /// Note: This should NOT be manipulated directly.
    /// </summary>
    public class AssetData
    {
        /// <summary>
        /// Identifier unique to this asset.
        /// </summary>
        [JsonName("id")]
        public string Guid;
        
        /// <summary>
        /// The URI at which to download the asset. This is not a complete URI
        /// but used to create a complete URI.
        /// </summary>
        [JsonName("uri")]
        public string Uri;

        /// <summary>
        /// Name of the asset in the bundle.
        /// </summary>
        [JsonName("name")]
        public string AssetName;

        /// <summary>
        /// Version of the asset.
        /// </summary>
        [JsonName("version")]
        public int Version;

        /// <summary>
        /// Crc for checking download validity.
        /// </summary>
        [JsonName("crc")]
        public string Crc;

        /// <summary>
        /// Tags associated with this asset.
        /// </summary>
        [JsonName("tags")]
        public string Tags;

        /// <summary>
        /// Stats associated with this asset, if any.
        /// </summary>
        [JsonName("stats")]
        public AssetStats Stats = new AssetStats();
        
        /// <summary>
        /// Useful ToString.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("[AssetInfo Guid={0}, AssetName={1}, Uri={2}]",
                Guid,
                AssetName,
                Uri);
        }
    }
}