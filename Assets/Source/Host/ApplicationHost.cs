using System;
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
        /// <param name="state"></param>
        /// <param name="http">Http service.</param>
        /// <param name="messages">The message router.</param>
        public ApplicationHost(
            IBridge bridge,
            IApplicationState state,
            IHttpService http,
            IMessageRouter messages)
        {
            _bridge = bridge;
            
            messages.Subscribe(
                MessageTypes.AUTHORIZED,
                _ =>
                {
                    Log.Info(this, "Application authorized.");

                    // setup http service
                    string userId;
                    if (!state.Get("user.profile.id", out userId))
                    {
                        throw new Exception("Could not get user id.");
                    }

                    string token;
                    if (!state.Get("user.credentials.token", out token))
                    {
                        throw new Exception("Could not get token.");
                    }

                    http.UrlBuilder.Replacements.Add(Commons.Unity.DataStructures.Tuple.Create(
                        "userId",
                        userId));
                    http.Headers.Add(Commons.Unity.DataStructures.Tuple.Create(
                        "Authorization",
                        string.Format("Bearer {0}", token)));
                });
        }

        /// <inheritdoc cref="IApplicationHost"/>
        public void Start()
        {
            _bridge.Initialize();

            // bind to events from web bridge
            _bridge.Binder.Add("state", MessageTypes.STATE);
            _bridge.Binder.Add("authorized", MessageTypes.AUTHORIZED);
            _bridge.Binder.Add("preview", MessageTypes.PREVIEW);

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