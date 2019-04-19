namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Optional event for MessageTypes.LOAD_APP.
    /// </summary>
    public class LoadAppEvent
    {
        /// <summary>
        /// If true, does not persist as most recent app in user preferences.
        /// </summary>
        public bool DoNotPersist;
    }
}
