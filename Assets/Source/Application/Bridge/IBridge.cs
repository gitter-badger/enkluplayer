namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Bridge between Unity and hosting application.
    /// </summary>
    public interface IBridge
    {
        /// <summary>
        /// Binds data.
        /// </summary>
        MessageTypeBinder Binder { get; }

        /// <summary>
        /// Initializes the bridge.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Uninitializes the bridge.
        /// </summary>
        void Uninitialize();

        /// <summary>
        /// Tells the hosting application that Unity application is ready. This
        /// must be called between Initialize and Uninitialize.
        /// </summary>
        void BroadcastReady();
    }
}