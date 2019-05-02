/**
 * NETIFY GENERATED CODE: DO NOT EDIT.
 */
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;

namespace CreateAR.Stargazer.Messages
{
    public class SessionsHttpController
    {
    
        private readonly IHttpService _http;
        
        public SessionsHttpController(IHttpService http)
        {
            _http = http;
        }

        public IAsyncToken<HttpResponse<CreateAR.Stargazer.Messages.CreateSession.Response>> CreateSession(CreateAR.Stargazer.Messages.CreateSession.Request request)
        {   
            // Headers: [ Authorization ]
            return _http.Post<CreateAR.Stargazer.Messages.CreateSession.Response>(
                "stargazer://" + "/sessions",
                request);
        }
    }
}