namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Runs services that run for the lifetime of the application.
    /// </summary>
    public interface IApplicationServiceManager
    {
        /// <summary>
        /// Starts application-wide services.
        /// </summary>
        void Start();

        /// <summary>
        /// Updates every frame.
        /// </summary>
        /// <param name="dt">Delta since last frame.</param>
        void Update(float dt);

        /// <summary>
        /// Call when the application is being shutdown.
        /// </summary>
        void Stop();

        /// <summary>
        /// Called when the application is suspended.
        /// </summary>
        void Suspend();

        /// <summary>
        /// Called when the application is resumed.
        /// </summary>
        void Resume();
    }
}