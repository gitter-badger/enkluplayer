using System.Collections.ObjectModel;
using CreateAR.Commons.Unity.Async;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Describes an object that manages <c>SceneController</c> instances.
    /// </summary>
    public interface IAppController
    {
        /// <summary>
        /// The scenes currently managed.
        /// </summary>
        ReadOnlyCollection<SceneController> Scenes { get; }

        /// <summary>
        /// Gets/sets the active <c>Scene</c>. This is the Scene that new
        /// elements will be added to, or elements will be deleted from.
        /// </summary>
        SceneController Active { get; set; }

        /// <summary>
        /// Initializes the manager. This should be called before any other calls.
        /// </summary>
        /// <param name="appId">The id of the app.</param>
        /// <returns></returns>
        IAsyncToken<Void> Initialize(string appId);

        /// <summary>
        /// Uninitializes the manager.
        /// </summary>
        void Uninitialize();

        /// <summary>
        /// Creates a <c>Scene</c>.
        /// </summary>
        /// <returns></returns>
        IAsyncToken<SceneController> Create();

        /// <summary>
        /// Destroys a <c>Scene</c> by id.
        /// </summary>
        /// <param name="id">The id of the <c>Scene</c>.</param>
        /// <returns></returns>
        IAsyncToken<SceneController> Destroy(string id);
    }
}