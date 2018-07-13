using System;
using CreateAR.Commons.Unity.Async;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Describes an object that controls an App.
    /// </summary>
    public interface IAppController
    {
        /// <summary>
        /// Current App id.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// True iff we have edit permissions.
        /// </summary>
        bool CanEdit { get; }

        /// <summary>
        /// True iff we have delete permissions.
        /// </summary>
        bool CanDelete { get; }

        /// <summary>
        /// Called when app is sufficiently loaded.
        /// </summary>
        event Action OnReady;

        /// <summary>
        /// Retrieves the object that manages the app's scenes.
        /// </summary>
        IAppSceneManager Scenes { get; }

        /// <summary>
        /// Loads the app.
        /// </summary>
        /// <param name="appId">The id of the app.</param>
        /// <returns></returns>
        IAsyncToken<Void> Load(string appId);

        /// <summary>
        /// Unloads the app.
        /// </summary>
        void Unload();

        /// <summary>
        /// Plays the app.
        /// </summary>
        void Play();
    }
}