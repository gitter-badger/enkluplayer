using System;
using CreateAR.Commons.Unity.Async;
using CreateAR.EnkluPlayer.IUX;
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
        /// Apply current instance's delta to static data in preparation for play.
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

        void Sync(ElementSchemaProp prop);

        void UnSync(ElementSchemaProp prop);

        void Own(string elementId, Action<bool> callback);

        void Forfeit(string elementId);
    }
}