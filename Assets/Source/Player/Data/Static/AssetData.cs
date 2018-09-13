using LightJson;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Bounds object used internally to <c>AssetData</c>.
    /// </summary>
    public class AssetStatsBoundsData
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
    public class AssetStatsData
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
        public AssetStatsBoundsData Bounds;
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
        /// Description.
        /// </summary>
        [JsonName("description")]
        public string Description;

        /// <summary>
        /// Id of owning user.
        /// </summary>
        [JsonName("owner")]
        public string Owner;

        /// <summary>
        /// Owning app.
        /// </summary>
        [JsonName("app")]
        public string App;

        /// <summary>
        /// The URI at which to download the asset. This is not a complete URI
        /// but used to create a complete URI.
        /// </summary>
        [JsonName("uri")]
        public string Uri;

        /// <summary>
        /// The URI at which to download the asset thumbnail. This is not a
        /// complete URI but used to create a complete URI.
        /// </summary>
        [JsonName("uriThumb")]
        public string UriThumb;

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
        public AssetStatsData Stats = new AssetStatsData();

        /// <summary>
        /// TODO: Remove unused field.
        /// </summary>
        [JsonName("status")]
        public string Status;

        /// <summary>
        /// Type of asset.
        /// </summary>
        [JsonName("type")]
        public string Type;

        /// <summary>
        /// Time at which the asset was created.
        /// </summary>
        [JsonName("createdAt")]
        public string CreatedAt;

        /// <summary>
        /// Last updated time.
        /// </summary>
        [JsonName("updatedAt")]
        public string UpdatedAt;

        /// <summary>
        /// iOS import status.
        /// </summary>
        [JsonName("ios")]
        public string Ios;

        /// <summary>
        /// Webgl import status.
        /// </summary>
        [JsonName("webgl")]
        public string Webgl;

        /// <summary>
        /// WsaPlayer import status.
        /// </summary>
        [JsonName("wsaplayer")]
        public string Wsaplayer;
        
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