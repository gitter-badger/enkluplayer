using System.Collections.ObjectModel;
using CreateAR.Commons.Unity.Async;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Describes an object that can update an app.
    /// </summary>
    public interface IAdminAppController : IAppController
    {
        /// <summary>
        /// The scenes currently managed.
        /// </summary>
        ReadOnlyCollection<SceneDesignController> Scenes { get; }

        /// <summary>
        /// Gets/sets the active <c>Scene</c>. This is the Scene that new
        /// elements will be added to, or elements will be deleted from.
        /// </summary>
        SceneDesignController Active { get; set; }

        /// <summary>
        /// Creates a <c>Scene</c>.
        /// </summary>
        /// <returns></returns>
        IAsyncToken<SceneDesignController> Create();

        /// <summary>
        /// Destroys a <c>Scene</c> by id.
        /// </summary>
        /// <param name="id">The id of the <c>Scene</c>.</param>
        /// <returns></returns>
        IAsyncToken<SceneDesignController> Destroy(string id);
    }
}