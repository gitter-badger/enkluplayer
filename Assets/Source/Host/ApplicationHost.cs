using System;
using CreateAR.Commons.Unity.DataStructures;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Spire;
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
        /// Makes Http Requests.
        /// </summary>
        private readonly IHttpService _http;

        /// <summary>
        /// Message router.
        /// </summary>
        private readonly IMessageRouter _messages;

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
            _http = http;
            _messages = messages;

            _messages.Subscribe(
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

                    _http.UrlBuilder.Replacements.Add(Tuple.Create(
                        "userId",
                        userId));
                    _http.Headers.Add(Tuple.Create(
                        "Authorization",
                        string.Format("Bearer {0}", token)));

                    // ready
                    _messages.Publish(MessageTypes.EDIT, Void.Instance);
                });
        }

        /// <inheritdoc cref="IApplicationHost"/>
        public void Ready()
        {
            // bind to events from web bridge
            _bridge.Binder.Add("state", MessageTypes.STATE);
            _bridge.Binder.Add("authorized", MessageTypes.AUTHORIZED);
            _bridge.Binder.Add("preview", MessageTypes.PREVIEW);

            // tell the webpage
            _bridge.BroadcastReady();
        }
    }
}