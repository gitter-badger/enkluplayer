using System;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Messaging;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// <c>IAssetUpdateService</c> implementation for the web player.
    /// </summary>
    public class WebAssetUpdateService : IAssetUpdateService
    {
        /// <summary>
        /// Dependencies.
        /// </summary>
        private readonly IMessageRouter _messages;

        /// <summary>
        /// Unsubscribe actions.
        /// </summary>
        private Action _addedUnsub;
        private Action _updatedUnsub;
        private Action _removedUnsub;

        /// <inheritdoc cref="IAssetUpdateService"/>
        public event Action<AssetData[]> OnAdded;

        /// <inheritdoc cref="IAssetUpdateService"/>
        public event Action<AssetData[]> OnUpdated;

        /// <inheritdoc cref="IAssetUpdateService"/>
        public event Action<AssetData[]> OnRemoved;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="messages">For messages.</param>
        public WebAssetUpdateService(IMessageRouter messages)
        {
            _messages = messages;
        }

        /// <inheritdoc cref="IAssetUpdateService"/>
        public IAsyncToken<Void> Initialize()
        {
            _addedUnsub = _messages.Subscribe(
                MessageTypes.ASSET_ADD,
                Messages_OnAssetAdded);
            _updatedUnsub = _messages.Subscribe(
                MessageTypes.ASSET_UPDATE,
                Messages_OnAssetUpdated);
            _removedUnsub = _messages.Subscribe(
                MessageTypes.ASSET_REMOVE,
                Messages_OnAssetRemoved);

            return new AsyncToken<Void>(Void.Instance);
        }

        /// <inheritdoc cref="IAssetUpdateService"/>
        public IAsyncToken<Void> Uninitialize()
        {
            _addedUnsub();
            _updatedUnsub();
            _removedUnsub();

            return new AsyncToken<Void>(Void.Instance);
        }

        /// <summary>
        /// Called when we receive an asset added message.
        /// </summary>
        /// <param name="message">Id of the asset.</param>
        private void Messages_OnAssetAdded(object message)
        {
            if (null != OnAdded)
            {
                OnAdded(null);
            }

            throw new NotImplementedException();
        }

        /// <summary>
        /// Called when we receive an asset updated message.
        /// </summary>
        /// <param name="message">Id of the asset.</param>
        private void Messages_OnAssetUpdated(object message)
        {
            if (null != OnUpdated)
            {
                OnUpdated(null);
            }

            throw new NotImplementedException();
        }

        /// <summary>
        /// Called when we receive an asset removed message.
        /// </summary>
        /// <param name="message">Id of the asset.</param>
        private void Messages_OnAssetRemoved(object message)
        {
            if (null != OnRemoved)
            {
                OnRemoved(null);
            }

            throw new NotImplementedException();
        }
    }
}