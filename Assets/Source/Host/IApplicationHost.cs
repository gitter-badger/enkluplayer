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
        /// <param name="delegate">The delegate which receives events pushed
        /// from the host.</param>
        void Ready(IApplicationHostDelegate @delegate);
    }
}