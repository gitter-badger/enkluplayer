using CreateAR.Commons.Unity.DataStructures;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;

namespace CreateAR.SpirePlayer
{
    public interface IApplicationHostDelegate
    {
        void On(int messageType, object message);
    }

    public interface IApplicationHost
    {
        void Ready(IApplicationHostDelegate @delegate);
    }

    public class EditorApplicationHost : IApplicationHost
    {
        private readonly IMessageRouter _messages;
        private readonly IHttpService _http;

        private IApplicationHostDelegate _delegate;

        public EditorApplicationHost(
            IMessageRouter messages,
            IHttpService http)
        {
            _messages = messages;
            _http = http;
        }

        public void Ready(IApplicationHostDelegate @delegate)
        {
            _delegate = @delegate;

            Log.Info(this, "Application is ready.");

            CreateUser();
        }

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

    public class WebApplicationHost : IApplicationHost
    {
        public void Ready(IApplicationHostDelegate @delegate)
        {
            Log.Info(this, "Application is ready.");

            throw new System.NotImplementedException();
        }
    }
}