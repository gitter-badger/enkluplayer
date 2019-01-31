namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Keys for metrics.
    /// </summary>
    public static class MetricsKeys
    {
        ///////////////////////////////////////////////////////////////////////
        /// Application
        ///////////////////////////////////////////////////////////////////////
        public const string APPLICATION_INIT = "Application.Creation";

        ///////////////////////////////////////////////////////////////////////
        /// App
        ///////////////////////////////////////////////////////////////////////
        public const string APP_DATA_LOAD = "App.Data.Load";
        public const string APP_PLAY = "App.Play";

        ///////////////////////////////////////////////////////////////////////
        /// States
        ///////////////////////////////////////////////////////////////////////
        public const string STATE_INIT = "State.Init";
        public const string STATE_LOGIN = "State.Login";
        public const string STATE_DEVICEREGISTRATION = "State.DeviceRegistration";
        public const string STATE_LOAD = "State.Load";
        public const string STATE_TIMETOPLAY = "State.TimeToPlay";

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

        public const string ANCHOR_TRACKING_LOST = "WorldAnchor.TrackingLost";
        
        public const string ANCHOR_STATE_UNLOCATEDRATIO = "WorldAnchor.State.UnlocatedRatio";
        public const string ANCHOR_STATE_LOCATEDRATIO = "WorldAnchor.State.LocatedRatio";

        ///////////////////////////////////////////////////////////////////////
        /// Scripts
        ///////////////////////////////////////////////////////////////////////
        public const string SCRIPT_DOWNLOADTIME = "Script.DownloadTime";
        public const string SCRIPT_LOADFROMCACHETIME = "Script.LoadFromCacheTime";
        public const string SCRIPT_PARSING_VINE = "Script.Parsing.Vine";
        public const string SCRIPT_PARSING_BEHAVIOR = "Script.Parsing.Behavior";

        ///////////////////////////////////////////////////////////////////////
        /// Assets
        ///////////////////////////////////////////////////////////////////////
        public const string ASSET_DL_QUEUE = "AssetDownload.Queued";
        public const string ASSET_DL_LOADING = "AssetDownload.Loading";
        public const string ASSET_DL_QUEUE_NONEMPTY = "AssetDownload.Queue.NonEmpty";
        public const string ASSET_DL_QUEUE_LENGTH = "AssetDownload.Queue.Length";

        ///////////////////////////////////////////////////////////////////////
        /// Performance
        ///////////////////////////////////////////////////////////////////////
        public const string PERF_FRAMETIME = "Perf.FrameTime";
        public const string PERF_MEMORY = "Perf.Memory";
        public const string PERF_PING = "Perf.Ping";
        public const string PERF_BATTERY = "Perf.Battery";
        public const string PERF_SESSION = "Perf.Session";
        
        ///////////////////////////////////////////////////////////////////////
        /// Media Capture
        ///////////////////////////////////////////////////////////////////////
        public const string MEDIA_IMAGE_START = "Media.Image.Start";
        public const string MEDIA_IMAGE_SUCCESS = "Media.Image.Success";
        public const string MEDIA_IMAGE_FAILURE = "Media.Image.Failure";
        public const string MEDIA_VIDEO_START = "Media.Video.Start";
        public const string MEDIA_VIDEO_SUCCESS = "Media.Video.Success";
        public const string MEDIA_VIDEO_FAILURE = "Media.Video.Failure";
    }
}