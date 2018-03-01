using CreateAR.Commons.Unity.Async;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Describes an object that can load an app.
    /// </summary>
    public interface IAppController
    {
        /// <summary>
        /// Initializes the manager. This should be called before any other calls.
        /// </summary>
        /// <param name="appId">The id of the app.</param>
        /// <param name="config">Config for play mode only.</param>
        /// <returns></returns>
        IAsyncToken<Void> Initialize(string appId, PlayModeConfig config);

        /// <summary>
        /// Uninitializes the manager.
        /// </summary>
        void Uninitialize();
    }
}