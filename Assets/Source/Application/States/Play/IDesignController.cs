using CreateAR.Commons.Unity.Async;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Design mode controller.
    /// </summary>
    public interface IDesignController
    {
        /// <summary>
        /// Starts controllers.
        /// </summary>
        void Setup(PlayModeConfig config, IAppController app);

        /// <summary>
        /// Tears down controller.
        /// </summary>
        void Teardown();

        /// <summary>
        /// Creates a scene and returns the scene id.
        /// </summary>
        /// <returns></returns>
        IAsyncToken<string> Create();
    }
}