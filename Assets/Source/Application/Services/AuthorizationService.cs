using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Takes care of authentication changes.
    /// </summary>
    public class AuthorizationService : ApplicationService
    {
        /// <summary>
        /// Makes http requests.
        /// </summary>
        private readonly IHttpService _http;

        /// <summary>
        /// Constructor.
        /// </summary>
        public AuthorizationService(
            IBridge bridge,
            IMessageRouter messages,
            IHttpService http)
            : base(bridge, messages)
        {
            _http = http;
        }

        /// <inheritdoc cref="ApplicationService"/>
        public override void Start()
        {
            base.Start();
            
            Subscribe<AuthorizedEvent>(
                MessageTypes.AUTHORIZED,
                message =>
                {
                    Log.Info(this, "Application authorized.");

                    // DEMO
                    _http.UrlBuilder.BaseUrl = "192.168.130.212";

                    // setup http service
                    _http.UrlBuilder.Replacements.Add(Commons.Unity.DataStructures.Tuple.Create(
                        "userId",
                        message.profile.id));
                    _http.Headers.Add(Commons.Unity.DataStructures.Tuple.Create(
                        "Authorization",
                        string.Format("Bearer {0}", message.credentials.token)));
                });
        }
    }
}