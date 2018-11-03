using CreateAR.Commons.Unity.Async;
using CreateAR.EnkluPlayer.IUX;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Creates and manages scenes from app data.
    /// </summary>
    public interface IAppSceneManager
    {
        /// <summary>
        /// All scene ids.
        /// </summary>
        string[] All { get; }

        /// <summary>
        /// Initializes the manager for an app.
        /// </summary>
        /// <param name="appId">The id of the app.</param>
        /// <param name="appData">Data for an app.</param>
        /// <returns></returns>
        IAsyncToken<Void> Initialize(
            string appId,
            IAppDataLoader appData);

        /// <summary>
        /// Unintializes the manager.
        /// </summary>
        /// <returns></returns>
        IAsyncToken<Void> Uninitialize();

        /// <summary>
        /// Retrieves the root element of a scene.
        /// </summary>
        /// <param name="sceneId">The id of a scene.</param>
        /// <returns></returns>
        Element Root(string sceneId);
        
        /// <summary>
        /// Resolves when all scenes' elements have been initialized.
        /// </summary>
        IAsyncToken<Void> OnInitialized { get; }
    }
}