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
        /// Initializes the bridge and provides an object to pass messages to.
        /// </summary>
        /// <param name="handler">The object that receives messages from the bridge.
        /// These messages are unpacked, filtered, and pushed onto the application
        /// message router.</param>
        void Initialize(BridgeMessageHandler handler);

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