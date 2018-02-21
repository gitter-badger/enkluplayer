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
        /// Application wide configuration.
        /// </summary>
        private readonly ApplicationConfig _config;

        /// <summary>
        /// Makes http requests.
        /// </summary>
        private readonly IHttpService _http;

        /// <summary>
        /// Constructor.
        /// </summary>
        public AuthorizationService(
            ApplicationConfig config,
            MessageTypeBinder binder,
            IMessageRouter messages,
            IHttpService http)
            : base(binder, messages)
        {
            _config = config;
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
                    
                    // setup http service
                    _http.UrlBuilder.Replacements.Add(Commons.Unity.DataStructures.Tuple.Create(
                        "userId",
                        message.Profile.Id));
                    _http.Headers.Add(Commons.Unity.DataStructures.Tuple.Create(
                        "Authorization",
                        string.Format("Bearer {0}", message.Credentials.Token)));

                    // fill-out ApplicationConfig
                    var creds = _config.Network.Credentials(_config.Network.Current);
                    creds.UserId = message.Profile.Id;
                    creds.Token = message.Credentials.Token;
                    creds.Email = message.Profile.Email;
                });
        }
    }
}