using CreateAR.Commons.Unity.DataStructures;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Hosts application in editor.
    /// </summary>
    public class EditorApplicationHost : IApplicationHost
    {
        /// <summary>
        /// Performs Http requests.
        /// </summary>
        private readonly IHttpService _http;

        /// <summary>
        /// For sending/receiving messages.
        /// </summary>
        private readonly IMessageRouter _messages;
        
        /// <summary>
        /// User's profile.
        /// </summary>
        private UserProfileModel _profile;

        /// <summary>
        /// Creates a new IApplicationHost implementation in the Editor.
        /// </summary>
        public EditorApplicationHost(
            IHttpService http,
            IMessageRouter messages)
        {
            _http = http;
            _messages = messages;
        }

        /// <inheritdoc cref="IApplicationHost"/>
        public void Ready()
        {
            CreateUser();
        }

        /// <summary>
        /// Creates a User.
        /// 
        /// TODO: Cache user id so we don't create a new user every time.
        /// </summary>
        private void CreateUser()
        {
            var displayName = System.Environment.MachineName;
            _http
                .Post<Response<UserModel>>(
                    _http.UrlBuilder.Url("/user"),
                    new CreateUserRequest
                    {
                        displayName = displayName,
                        provider = "none",
                        providerToken = "none"
                    })
                .OnSuccess(response =>
                {
                    if (response.Payload.success)
                    {
                        Log.Debug(this, "Host successfully created a user.");

                        var userId = response.Payload.body.id;

                        _profile = new UserProfileModel
                        {
                            id = response.Payload.body.id,
                            displayName = response.Payload.body.displayName
                        };

                        // get a token
                        GetToken();
                    }
                    else
                    {
                        _messages.Publish(
                            MessageTypes.FATAL_ERROR,
                            new FatalErrorEvent
                            {
                                Error = response.Payload.error
                            });
                    }
                })
                .OnFailure(exception =>
                {
                    // push to application
                    _messages.Publish(
                        MessageTypes.FATAL_ERROR,
                        new FatalErrorEvent
                        {
                            Error = "Could not create a user."
                        });
                });
        }

        /// <summary>
        /// Retrieves the JWT and configures <c>IHttpService</c>.
        /// </summary>
        private void GetToken()
        {
            _http
                .Post<Response<GetTokenBody>>(
                    _http.UrlBuilder.Url("/user/{userId}/token"),
                    null)
                .OnSuccess(response =>
                {
                    if (response.Payload.success)
                    {
                        Log.Debug(this, "Host successfully retrieved token.");

                        var token = response.Payload.body.token;

                        _messages.Publish(
                            MessageTypes.AUTHORIZED,
                            new AuthorizedEvent
                            {
                                credentials = new UserCredentialsModel
                                {
                                    token = token
                                },
                                profile = _profile
                            });
                    }
                    else
                    {
                        _messages.Publish(
                            MessageTypes.FATAL_ERROR,
                            new FatalErrorEvent
                            {
                                Error = response.Payload.error
                            });
                    }
                })
                .OnFailure(exception =>
                {
                    _messages.Publish(
                        MessageTypes.FATAL_ERROR,
                        new FatalErrorEvent
                        {
                            Error = exception.Message
                        });
                });
        }
    }
}