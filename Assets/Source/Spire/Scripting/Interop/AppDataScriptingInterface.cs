namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Interface for interacting with app data.
    /// </summary>
    [JsInterface("appdata")]
    public class AppDataScriptingInterface
    {
        /// <summary>
        /// The IAppDataManager implementation.
        /// </summary>
        private readonly IAppDataManager _appData;

        /// <summary>
        /// True iff app data is ready.
        /// </summary>
        public bool IsReady { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="appData">The app data.</param>
        public AppDataScriptingInterface(IAppDataManager appData)
        {
            _appData = appData;
            _appData.OnLoaded += () => IsReady = true;
            _appData.OnUnloaded += () => IsReady = false;

            IsReady = true;
        }
    }
}