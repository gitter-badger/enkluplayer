namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Configuration.
    /// </summary>
    public class AppTxnConfiguration
    {
        /// <summary>
        /// Id of the app.
        /// </summary>
        public string AppId;

        /// <summary>
        /// App scenes.
        /// </summary>
        public IAppSceneManager Scenes;

        /// <summary>
        /// True iff authentication should be required for txns.
        /// </summary>
        public bool AuthenticateTxns = true;
    }
}