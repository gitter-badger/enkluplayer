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
        /// Delegate implementation.
        /// </summary>
        private IApplicationHostDelegate _delegate;

        /// <summary>
        /// Creates a new IApplicationHost implementation in the Editor.
        /// </summary>
        public EditorApplicationHost(IHttpService http)
        {
            _http = http;
        }

        /// <inheritdoc cref="IApplicationHost"/>
        public void Ready(IApplicationHostDelegate @delegate)
        {
            _delegate = @delegate;
            
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

                        // set the userid
                        _http.UrlBuilder.Replacements.Add(Tuple.Create(
                            "userId",
                            userId));

                        // get a token
                        GetToken();
                    }
                    else
                    {
                        _delegate.On(
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
                    _delegate.On(
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

                        _http.Headers.Add(Tuple.Create(
                            "Authorization",
                            string.Format("Bearer {0}", token)));

                        _delegate.On(
                            MessageTypes.AUTHORIZED,
                            new AuthorizedEvent());
                    }
                    else
                    {
                        _delegate.On(
                            MessageTypes.FATAL_ERROR,
                            new FatalErrorEvent
                            {
                                Error = response.Payload.error
                            });
                    }
                })
                .OnFailure(exception =>
                {
                    _delegate.On(
                        MessageTypes.FATAL_ERROR,
                        new FatalErrorEvent
                        {
                            Error = exception.Message
                        });
                });
        }
    }
}