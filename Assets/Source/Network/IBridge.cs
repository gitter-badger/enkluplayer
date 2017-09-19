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
        DataBinder Binder { get; }

        /// <summary>
        /// Tells the webpage that the application is ready.
        /// </summary>
        void BroadcastReady();
    }
}