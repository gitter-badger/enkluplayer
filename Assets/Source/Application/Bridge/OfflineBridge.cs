namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Bridge that does nothing.
    /// </summary>
    public class OfflineBridge : IBridge
    {
        /// <inheritdoc cref="IBridge"/>
        public MessageTypeBinder Binder { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public OfflineBridge()
        {
            Binder = new MessageTypeBinder();
        }

        /// <inheritdoc />
        public void Initialize(BridgeMessageHandler handler)
        {
            // nothing
        }

        /// <inheritdoc />
        public void Uninitialize()
        {
            Binder.Clear();
        }

        /// <inheritdoc />
        public void BroadcastReady()
        {
            // 
        }

        /// <inheritdoc />
        public void Send(string message)
        {
            //
        }
    }
}