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
            
            messages.Subscribe(
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
            _bridge.Binder.Add<AssetUpdateEvent>(MessageTypes.ASSET_UPDATE);
            _bridge.Binder.Add<ContentUpdateEvent>(MessageTypes.CONTENT_UPDATE);
            _bridge.Binder.Add<AuthorizedEvent>(MessageTypes.AUTHORIZED);
            _bridge.Binder.Add<PreviewEvent>(MessageTypes.PREVIEW);
            _bridge.Binder.Add<HierarchyEvent>(MessageTypes.HIERARCHY);
            _bridge.Binder.Add<SelectContentEvent>(MessageTypes.SELECT_CONTENT);

            // tell the webpage
            _bridge.BroadcastReady();
        }

        /// <inheritdoc cref="IApplicationHost"/>
        public void Stop()
        {
            _bridge.Binder.Clear();
            _bridge.Uninitialize();
        }
    }
}