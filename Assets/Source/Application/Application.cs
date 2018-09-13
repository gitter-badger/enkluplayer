namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Root application.
    /// </summary>
    public class Application
    {
        /// <summary>
        /// The host.
        /// </summary>
        private readonly IApplicationServiceManager _services;
        
        /// <summary>
        /// Creates a new Application.
        /// </summary>
        public Application(IApplicationServiceManager services)
        {
            _services = services;
        }
        
        /// <summary>
        /// Initializes the application.
        /// </summary>
        public void Initialize()
        {
            _services.Start();
        }

        /// <summary>
        /// Uninitializes the application.
        /// </summary>
        public void Uninitialize()
        {
            _services.Stop();
        }

        /// <summary>
        /// Called every frame.
        /// </summary>
        /// <param name="dt">The time since last time Update was called.</param>
        public void Update(float dt)
        {
            _services.Update(dt);
        }

        /// <summary>
        /// Called when the application is suspended.
        /// </summary>
        public void Suspend()
        {
            _services.Suspend();
        }

        /// <summary>
        /// Called when the application is resumed.
        /// </summary>
        public void Resume()
        {
            _services.Resume();
        }
    }
}