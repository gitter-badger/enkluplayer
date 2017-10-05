using CreateAR.Commons.Unity.Async;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Describes an interface for creating, querying, and destroying
    /// <c>Scene</c> instances.
    /// </summary>
    public interface ISceneManager
    {
        /// <summary>
        /// Searches for an active scene.
        /// </summary>
        /// <param name="id">Unique id of the scene.</param>
        /// <returns></returns>
        Scene Find(string id);

        /// <summary>
        /// Creates a new scene.
        /// </summary>
        /// <param name="id">Unique id of the scene.</param>
        /// <returns></returns>
        IAsyncToken<Scene> Load(string id);

        /// <summary>
        /// Destroys a scene.
        /// </summary>
        /// <param name="id">Unique id of the scene.</param>
        IAsyncToken<Void> Unload(string id);
    }
}