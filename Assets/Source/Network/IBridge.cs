using CreateAR.SpirePlayer;

namespace CreateAR.Spire
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
        /// Tells the hosting application that Unity application is ready.
        /// </summary>
        void BroadcastReady();
    }
}