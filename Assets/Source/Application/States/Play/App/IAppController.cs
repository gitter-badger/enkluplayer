using System;
using CreateAR.Commons.Unity.Async;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.EnkluPlayer
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
        /// Current App name.
        /// </summary>
        string Name { get; }
        
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
        /// <param name="config">configuration for play.</param>
        /// <returns></returns>
        IAsyncToken<Void> Load(PlayAppConfig config);

        /// <summary>
        /// Unloads the app.
        /// </summary>
        void Unload();

        /// <summary>
        /// Plays the app.
        /// </summary>
        void Play();

        /// <summary>
        /// Edits the app.
        /// </summary>
        void Edit();
    }
}