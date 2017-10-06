namespace CreateAR.SpirePlayer
{
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
        public string Guid;

        /// <summary>
        /// The URI at which to download the asset. This is not a complete URI
        /// but used to create a complete URI.
        /// </summary>
        public string Uri;

        /// <summary>
        /// Name of the asset in the bundle.
        /// </summary>
        public string AssetName;

        /// <summary>
        /// Version of the asset.
        /// </summary>
        public int Version;

        /// <summary>
        /// Crc for checking download validity.
        /// </summary>
        public string Crc;

        /// <summary>
        /// Tags associated with this asset.
        /// </summary>
        public string[] Tags;

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