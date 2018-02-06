using System.Collections.ObjectModel;
using CreateAR.Commons.Unity.Async;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Describes an object that manages <c>PropSet</c> instances.
    /// </summary>
    public interface IPropManager
    {
        /// <summary>
        /// The sets currently managed.
        /// </summary>
        ReadOnlyCollection<PropSet> Sets { get; }

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
        /// Creates a <c>PropSet</c>.
        /// </summary>
        /// <returns></returns>
        IAsyncToken<PropSet> Create();

        /// <summary>
        /// Destroys a <c>PropSet</c> by id.
        /// </summary>
        /// <param name="id">The id of the <c>PropSet</c>.</param>
        /// <returns></returns>
        IAsyncToken<PropSet> Destroy(string id);
    }
}