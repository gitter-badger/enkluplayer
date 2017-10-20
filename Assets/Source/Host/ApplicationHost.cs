using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;

using Void = CreateAR.Commons.Unity.Async.Void;

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
        /// Application data.
        /// </summary>
        private readonly IAdminAppDataManager _appData;

        /// <summary>
        /// Manages assets.
        /// </summary>
        private readonly IAssetManager _assets;

        /// <summary>
        /// Graphs relationships between content.
        /// </summary>
        private readonly ContentGraph _contentGraph;

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
        /// <param name="appData">Application data.</param>
        /// <param name="assets">Manages assets.</param>
        /// <param name="contentGraph">Graph of content.</param>
        public ApplicationHost(
            IBridge bridge,
            IHttpService http,
            IMessageRouter messages,
            IAdminAppDataManager appData,
            IAssetManager assets,
            ContentGraph contentGraph)
        {
            _bridge = bridge;
            _messages = messages;
            _appData = appData;
            _assets = assets;
            _contentGraph = contentGraph;

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

            // states
            _bridge.Binder.Add<PreviewEvent>(MessageTypes.PREVIEW);
            _bridge.Binder.Add<Void>(MessageTypes.HIERARCHY);

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

        /// <summary>
        /// Adds a binding + a message handler for a messagetype.
        /// </summary>
        /// <typeparam name="T">Type to deserialize the message into.</typeparam>
        /// <param name="messageType">Type of message.</param>
        /// <param name="handler">Handler to handle the message.</param>
        private void Subscribe<T>(int messageType, Action<T> handler)
        {
            _bridge.Binder.Add<T>(messageType);

            _unsubscribeList.Add(_messages.Subscribe(
                messageType,
                @event => handler((T) @event)));
        }

        /// <summary>
        /// Adds subscriptions for Asset events.
        /// 
        /// TODO: Move?
        /// </summary>
        private void AddAssetSubscriptions()
        {
            Subscribe<AssetListEvent>(MessageTypes.ASSET_LIST, Messages_OnAssetList);
            Subscribe<AssetAddEvent>(MessageTypes.ASSET_ADD, Messages_OnAssetAdd);
            Subscribe<AssetRemoveEvent>(MessageTypes.ASSET_REMOVE, Messages_OnAssetRemove);
            Subscribe<AssetUpdateEvent>(MessageTypes.ASSET_UPDATE, Messages_OnAssetUpdate);
        }

        /// <summary>
        /// Adds subscriptions for Content events.
        /// 
        /// TODO: Move?
        /// </summary>
        private void AddContentSubscriptions()
        {
            Subscribe<ContentListEvent>(MessageTypes.CONTENT_LIST, @event =>
            {
                Log.Info(this,
                    "Updating ContentData with {0} instances.",
                    @event.Content.Length);

                _appData.Set(@event.Content);
            });

            Subscribe<ContentAddEvent>(MessageTypes.CONTENT_ADD, @event =>
            {
                Log.Info(this, "Add content.");

                _appData.Add(@event.Content);
            });

            Subscribe<ContentRemoveEvent>(MessageTypes.CONTENT_REMOVE, @event =>
            {
                Log.Info(this, "Remove content.");

                var content = _appData.Get<ContentData>(@event.Id);
                if (null != content)
                {
                    _appData.Remove(content);
                }
                else
                {
                    Log.Warning(this, "Remove content request was for content of unknown id.");
                }
            });

            Subscribe<ContentUpdateEvent>(MessageTypes.CONTENT_UPDATE, @event =>
            {
                Log.Info(this, "Update content.");

                // update app data (the hierarchy is listening for updates)
                _appData.Update(@event.Content);
            });
        }

        /// <summary>
        /// Adds subscriptions for Hierarchy events.
        /// 
        /// TODO: Move?
        /// </summary>
        private void AddHierarchySubscriptions()
        {
            _bridge.Binder.Add<HierarchySelectEvent>(MessageTypes.HIERARCHY_SELECT);

            Subscribe<HierarchyListEvent>(MessageTypes.HIERARCHY_LIST, @event =>
            {
                Log.Info(this, "Hierarchy list updated.");
                
                _contentGraph.Add(
                    _contentGraph.Root.Id,
                    @event.Children);

                _contentGraph.Walk(node =>
                {
                    Log.Info(this, node);
                });
            });
            
            Subscribe<HierarchyAddEvent>(MessageTypes.HIERARCHY_ADD, @event =>
            {
                Log.Info(this, "Add node to hierarchy.");
                
                _contentGraph.Add(@event.Parent, @event.Node);

                _contentGraph.Walk(node =>
                {
                    Log.Info(this, node);
                });
            });

            Subscribe<HierarchyRemoveEvent>(MessageTypes.HIERARCHY_REMOVE, @event =>
            {
                Log.Info(this, "Remove node from hierarchy.");

                _contentGraph.Remove(@event.ContentId);
            });

            Subscribe<HierarchyUpdateEvent>(MessageTypes.HIERARCHY_UPDATE, @event =>
            {
                Log.Info(this, "Hierarchy node updated.");

                _contentGraph.Update(@event.Node);
            });
        }

        /// <summary>
        /// Called when an <c>Asset</c> has been updated.
        /// </summary>
        /// <param name="event">The event.</param>
        private void Messages_OnAssetUpdate(AssetUpdateEvent @event)
        {
            Log.Info(this, "Update asset.");

            // ImportService marks AssetNames with guid
            @event.Asset.AssetName = @event.Asset.Guid;
            _assets.Manifest.Update(@event.Asset);
        }

        /// <summary>
        /// Called when an <c>Asset</c> has been removed.
        /// </summary>
        /// <param name="event">The event.</param>
        private void Messages_OnAssetRemove(AssetRemoveEvent @event)
        {
            Log.Info(this, "Remove asset.");

            _assets.Manifest.Remove(@event.Id);
        }

        /// <summary>
        /// Called when an <c>Asset</c> has been added.
        /// </summary>
        /// <param name="event">The event.</param>
        private void Messages_OnAssetAdd(AssetAddEvent @event)
        {
            Log.Info(this, "Add asset.");

            // ImportService marks AssetNames with guid
            @event.Asset.AssetName = @event.Asset.Guid;
            _assets.Manifest.Add(@event.Asset);
        }

        /// <summary>
        /// Called when a complete manifest of assets has been sent over.
        /// </summary>
        /// <param name="event">The event.</param>
        private void Messages_OnAssetList(AssetListEvent @event)
        {
            Log.Info(this,
                "Updating AssetManifest with {0} assets.",
                @event.Assets.Length);

            // handle adds + updates
            var manifest = _assets.Manifest;
            var assets = @event.Assets;
            for (int i = 0, len = assets.Length; i < len; i++)
            {
                var asset = assets[i];

                // ImportService marks AssetNames with guid
                asset.AssetName = asset.Guid;

                var info = manifest.Data(asset.Guid);
                if (null == info)
                {
                    _assets.Manifest.Add(asset);
                }
                else
                {
                    manifest.Update(asset);
                }
            }

            // handle removes
            var all = manifest.All;
            for (int i = 0, ilen = all.Length; i < ilen; i++)
            {
                var data = all[i];

                var found = false;
                for (int j = 0, jlen = assets.Length; j < jlen; j++)
                {
                    var asset = assets[i];
                    if (data.Guid == asset.Guid)
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    manifest.Remove(data.Guid);
                }
            }
        }
    }
}