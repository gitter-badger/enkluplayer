using System;
using System.Linq;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// <c>IAssetUpdateService</c> implementation in the Unity Editor/
    /// </summary>
    public class EditorAssetUpdateService : IAssetUpdateService
    {
        /// <summary>
        /// Dependencies.
        /// </summary>
        private readonly IHttpService _http;
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
        /// <param name="http">To make HTTP requests.</param>
        /// <param name="messages">For pub/sub.</param>
        public EditorAssetUpdateService(
            IHttpService http,
            IMessageRouter messages)
        {
            _http = http;
            _messages = messages;
        }

        /// <inheritdoc cref="IAssetUpdateService"/>
        public IAsyncToken<Void> Initialize()
        {
            var token = new AsyncToken<Void>();

            _addedUnsub = _messages.Subscribe(
                MessageTypes.ASSET_ADD,
                Messages_OnAssetAdded);
            _updatedUnsub = _messages.Subscribe(
                MessageTypes.ASSET_UPDATE,
                Messages_OnAssetUpdated);
            _removedUnsub = _messages.Subscribe(
                MessageTypes.ASSET_REMOVE,
                Messages_OnAssetRemoved);

            Log.Info(this, "Get Asset Manifest.");

            _http
                .Get<Trellis.Messages.GetAssets.Response>(_http.UrlBuilder.Url("/asset"))
                .OnSuccess(response =>
                {
                    Log.Info(this, "Got manifest.");

                    if (null == response.Payload.Body
                        || null == response.Payload.Body.Assets)
                    {
                        Log.Error(
                            this,
                            "Improper response. No assets on asset manifest request.");
                        return;
                    }

                    if (null != OnAdded)
                    {
                        var assetInfos = response.Payload.Body
                            .Assets
                            .Select(asset => new AssetData
                            {
                                Guid = asset.Id,
                                Uri = ""
                            })
                            .ToArray();

                        OnAdded(assetInfos);
                    }

                    token.Succeed(Void.Instance);
                })
                .OnFailure(token.Fail);

            return token;
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