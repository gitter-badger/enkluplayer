namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Keys for metrics.
    /// </summary>
    public static class MetricsKeys
    {
        ///////////////////////////////////////////////////////////////////////
        /// App
        ///////////////////////////////////////////////////////////////////////
        public const string APP_DATA_LOAD = "App.Data.Load";

        ///////////////////////////////////////////////////////////////////////
        /// Anchors
        ///////////////////////////////////////////////////////////////////////
        public const string ANCHOR_EXPORT = "WorldAnchor.Export";
        public const string ANCHOR_COMPRESSION = "WorldAnchor.Compression";
        public const string ANCHOR_UPLOAD = "WorldAnchor.Upload";

        public const string ANCHOR_DOWNLOAD = "WorldAnchor.Download";
        public const string ANCHOR_EXPORT_QUEUED = "WorldAnchor.Export.Queued";
        public const string ANCHOR_DECOMPRESSION = "WorldAnchor.Decompression";
        public const string ANCHOR_IMPORT = "WorldAnchor.Import";

        public const string ANCHOR_UNLOCATED = "WorldAnchor.Unlocated";

        public const string ANCHOR_SIZE_RAW = "WorldAnchor.Size.Raw";
        public const string ANCHOR_SIZE_COMPRESSED = "WorldAnchor.Size.Compressed";
        public const string ANCHOR_SIZE_RATIO = "WorldAnchor.Size.Ratio";
    }
}