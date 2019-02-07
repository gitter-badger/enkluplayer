using System;
using CreateAR.Commons.Unity.Async;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Describes a multiplayer interface.
    ///
    /// Initialize() -> ApplyDiff() -> Play()
    ///
    /// </summary>
    public interface IMultiplayerController
    {
        /// <summary>
        /// Preps the multiplayer controller.
        /// </summary>
        /// <returns></returns>
        IAsyncToken<Void> Initialize();
        
        /// <summary>
        /// Apply data changes in preparation for play.
        /// </summary>
        void ApplyDiff(IAppDataLoader appData);

        /// <summary>
        /// Play mode applies changes to active elements.
        /// </summary>
        void Play();

        /// <summary>
        /// Disconnects from the multiplayer server.
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Sends a message to the multiplayer server.
        ///
        /// This can be called on any thread.
        /// </summary>
        /// <param name="message">The message to send.</param>
        void Send(object message);

        /// <summary>
        /// Subscribes to a message from the multiplayer server.
        /// </summary>
        /// <typeparam name="T">The type of message to listen to.</typeparam>
        /// <param name="callback">The function to call when the message has been received.</param>
        void Subscribe<T>(Action<T> callback);

        /// <summary>
        /// Unsubscribes a handler from the multiplayer server.
        /// </summary>
        /// <typeparam name="T">The type of message to stop listening to.</typeparam>
        void UnSubscribe<T>();
    }
}