using System;
using CreateAR.Commons.Unity.Async;
using CreateAR.EnkluPlayer.IUX;
using Enklu.Data;
using Enklu.Mycelium.Messages.Experience;
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
        /// True iff we are connected to multiplayer.
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Called when the connection has changed.
        /// </summary>
        event Action<bool> OnConnectionChanged;

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

        /// <summary>
        /// Creates an element.
        /// </summary>
        /// <param name="parentId"></param>
        /// <param name="element"></param>
        /// <param name="expiration"></param>
        /// <param name="owner"></param>
        IAsyncToken<Element> Create(
            string parentId,
            ElementData element,
            string owner = null,
            ElementExpirationType expiration = ElementExpirationType.Session);

        /// <summary>
        /// Deletes an element.
        /// </summary>
        /// <param name="id">The id of the element.</param>
        /// <returns></returns>
        IAsyncToken<Void> Destroy(string id);

        /// <summary>
        /// Toggles a prop on an element and sets a timer for flipping back.
        /// </summary>
        /// <param name="elementId">The id of the element.</param>
        /// <param name="prop">The name of the prop.</param>
        /// <param name="value">The value to set the prop to.</param>
        /// <param name="milliseconds">The number of milliseconds to wait before revert.</param>
        void AutoToggle(string elementId, string prop, bool value, int milliseconds);

        void Sync(ElementSchemaProp prop);

        void UnSync(ElementSchemaProp prop);

        void Own(string elementId, Action<bool> callback);

        void Forfeit(string elementId);
    }
}