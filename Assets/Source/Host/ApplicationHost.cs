using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// <c>IApplicationHost</c> implementation for webpages.
    /// </summary>
    public class ApplicationHost : IApplicationHost
    {
        /// <summary>
        /// The bridge into the web world.
        /// </summary>
        private readonly IBridge _bridge;

        /// <summary>
        /// Messages.
        /// </summary>
        private readonly IMessageRouter _messages;

        /// <summary>
        /// List of methods to unsubscribe.
        /// </summary>
        private readonly List<Action> _unsubscribeList = new List<Action>();

        /// <summary>
        /// Creates a new WebApplicationHost.
        /// </summary>
        /// <param name="bridge">The WebBridge.</param>
        /// <param name="http">Http service.</param>
        /// <param name="messages">The message router.</param>
        public ApplicationHost(
            IBridge bridge,
            IHttpService http,
            IMessageRouter messages)
        {
            _bridge = bridge;
            _messages = messages;
            
            // TODO: Move to Application.
            _messages.Subscribe(
                MessageTypes.AUTHORIZED,
                @event =>
                {
                    var message = (AuthorizedEvent) @event;

                    Log.Info(this, "Application authorized.");

                    // setup http service
                    http.UrlBuilder.Replacements.Add(Commons.Unity.DataStructures.Tuple.Create(
                        "userId",
                        message.profile.id));
                    http.Headers.Add(Commons.Unity.DataStructures.Tuple.Create(
                        "Authorization",
                        string.Format("Bearer {0}", message.credentials.token)));
                });
        }

        /// <inheritdoc cref="IApplicationHost"/>
        public void Start()
        {
            _bridge.Initialize();

            // bind to events from web bridge
            _bridge.Binder.Add<AuthorizedEvent>(MessageTypes.AUTHORIZED);
            _bridge.Binder.Add<PreviewEvent>(MessageTypes.PREVIEW);

            // subscribe
            AddAssetSubscriptions();
            AddContentSubscriptions();
            AddHierarchySubscriptions();

            // tell the webpage
            _bridge.BroadcastReady();
        }

        /// <inheritdoc cref="IApplicationHost"/>
        public void Stop()
        {
            for (int i = 0, len = _unsubscribeList.Count; i < len; i++)
            {
                _unsubscribeList[i]();
            }
            _unsubscribeList.Clear();

            _bridge.Binder.Clear();
            _bridge.Uninitialize();
        }

        private void Subscribe<T>(int messageType, Action<T> handler)
        {
            _bridge.Binder.Add<T>(messageType);

            _unsubscribeList.Add(_messages.Subscribe(
                messageType,
                @event => handler((T) @event)));
        }

        private void AddAssetSubscriptions()
        {
            Subscribe<AssetListEvent>(MessageTypes.ASSET_LIST, @event =>
            {
                Log.Info(this, "Asset list updated.");
            });

            Subscribe<AssetAddEvent>(MessageTypes.ASSET_ADD, @event =>
            {
                Log.Info(this, "Add asset.");
            });

            Subscribe<AssetRemoveEvent>(MessageTypes.ASSET_REMOVE, @event =>
            {
                Log.Info(this, "Remove asset.");
            });

            Subscribe<AssetUpdateEvent>(MessageTypes.ASSET_UPDATE, @event =>
            {
                Log.Info(this, "Update asset.");
            });
        }

        private void AddContentSubscriptions()
        {
            Subscribe<ContentListEvent>(MessageTypes.CONTENT_LIST, @event =>
            {
                Log.Info(this, "Content list updated.");
            });

            Subscribe<ContentAddEvent>(MessageTypes.CONTENT_ADD, @event =>
            {
                Log.Info(this, "Add content.");
            });

            Subscribe<ContentRemoveEvent>(MessageTypes.CONTENT_REMOVE, @event =>
            {
                Log.Info(this, "Remove content.");
            });

            Subscribe<ContentUpdateEvent>(MessageTypes.CONTENT_UPDATE, @event =>
            {
                Log.Info(this, "Update content.");
            });
        }

        private void AddHierarchySubscriptions()
        {
            Subscribe<HierarchySelectEvent>(MessageTypes.HIERARCHY_SELECT, @event =>
            {
                Log.Info(this, "Select hierarchy event.");
            });

            Subscribe<HierarchyAddEvent>(MessageTypes.HIERARCHY_ADD, @event =>
            {
                Log.Info(this, "Add node to hierarchy.");
            });

            Subscribe<HierarchyRemoveEvent>(MessageTypes.HIERARCHY_REMOVE, @event =>
            {
                Log.Info(this, "Remove node from hierarchy.");
            });

            Subscribe<HierarchyUpdateEvent>(MessageTypes.HIERARCHY_UPDATE, @event =>
            {
                Log.Info(this, "Hierarchy node updated.");
            });

            Subscribe<HierarchyListEvent>(MessageTypes.HIERARCHY_LIST, @event =>
            {
                Log.Info(this, "Hierarchy list updated.");
            });
        }
    }
}