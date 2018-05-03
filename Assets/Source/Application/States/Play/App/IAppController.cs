using CreateAR.Commons.Unity.Async;

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