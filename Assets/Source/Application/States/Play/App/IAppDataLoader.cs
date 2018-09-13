using CreateAR.Commons.Unity.Async;
using CreateAR.EnkluPlayer.IUX;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Describes an object that knows how to load all data for an app.
    /// </summary>
    public interface IAppDataLoader
    {
        /// <summary>
        /// List of all scene ids in this app.
        /// </summary>
        string[] Scenes { get; }

        /// <summary>
        /// Retrieves the full data description of a scene.
        /// </summary>
        /// <param name="sceneId">The id of the scene to retrieve.</param>
        /// <returns></returns>
        ElementDescription Scene(string sceneId);

        /// <summary>
        /// Loads all app data.
        /// </summary>
        /// <param name="appId">The id of the app.</param>
        /// <returns></returns>
        IAsyncToken<Void> Load(string appId);

        /// <summary>
        /// Unloads all app data.
        /// </summary>
        void Unload();
    }
}