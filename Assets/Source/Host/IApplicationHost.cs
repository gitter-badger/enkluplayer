namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Hosts an application.
    /// </summary>
    public interface IApplicationHost
    {
        /// <summary>
        /// Call when the application is ready. Before this point, the application
        /// will not receive any events from the host.
        /// </summary>
        void Start();

        /// <summary>
        /// Call when the application is being shutdown.
        /// </summary>
        void Stop();
    }
}