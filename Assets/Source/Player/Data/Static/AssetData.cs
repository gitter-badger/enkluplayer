using Newtonsoft.Json;

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
        [JsonProperty("min")]
        public Vec3 Min;

        /// <summary>
        /// Maximum.
        /// </summary>
        [JsonProperty("max")]
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
        [JsonProperty("vertCount")]
        public int VertCount;

        /// <summary>
        /// Tri count.
        /// </summary>
        [JsonProperty("triCount")]
        public int TriCount;

        /// <summary>
        /// Bounds.
        /// </summary>
        [JsonProperty("bounds")]
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
        [JsonProperty("id")]
        public string Guid;

        /// <summary>
        /// Description.
        /// </summary>
        [JsonProperty("description")]
        public string Description;

        /// <summary>
        /// Id of owning user.
        /// </summary>
        [JsonProperty("owner")]
        public string Owner;

        /// <summary>
        /// Owning app.
        /// </summary>
        [JsonProperty("app")]
        public string App;

        /// <summary>
        /// The URI at which to download the asset. This is not a complete URI
        /// but used to create a complete URI.
        /// </summary>
        [JsonProperty("uri")]
        public string Uri;

        /// <summary>
        /// The URI at which to download the asset thumbnail. This is not a
        /// complete URI but used to create a complete URI.
        /// </summary>
        [JsonProperty("uriThumb")]
        public string UriThumb;

        /// <summary>
        /// Name of the asset in the bundle.
        /// </summary>
        [JsonProperty("name")]
        public string AssetName;

        /// <summary>
        /// Version of the asset.
        /// </summary>
        [JsonProperty("version")]
        public int Version;

        /// <summary>
        /// Crc for checking download validity.
        /// </summary>
        [JsonProperty("crc")]
        public string Crc;

        /// <summary>
        /// Tags associated with this asset.
        /// </summary>
        [JsonProperty("tags")]
        public string Tags;

        /// <summary>
        /// Stats associated with this asset, if any.
        /// </summary>
        [JsonProperty("stats")]
        public AssetStatsData Stats = new AssetStatsData();

        /// <summary>
        /// TODO: Remove unused field.
        /// </summary>
        [JsonProperty("status")]
        public string Status;

        /// <summary>
        /// Type of asset.
        /// </summary>
        [JsonProperty("type")]
        public string Type;

        /// <summary>
        /// Time at which the asset was created.
        /// </summary>
        [JsonProperty("createdAt")]
        public string CreatedAt;

        /// <summary>
        /// Last updated time.
        /// </summary>
        [JsonProperty("updatedAt")]
        public string UpdatedAt;

        /// <summary>
        /// iOS import status.
        /// </summary>
        [JsonProperty("ios")]
        public string Ios;

        /// <summary>
        /// Webgl import status.
        /// </summary>
        [JsonProperty("webgl")]
        public string Webgl;

        /// <summary>
        /// WsaPlayer import status.
        /// </summary>
        [JsonProperty("wsaplayer")]
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