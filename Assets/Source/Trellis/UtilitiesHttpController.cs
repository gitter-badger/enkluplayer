/**
 * NETIFY GENERATED CODE: DO NOT EDIT.
 */
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;

namespace CreateAR.Trellis.Messages
{
    public class UtilitiesHttpController
    {
    
        private readonly IHttpService _http;
        
        public UtilitiesHttpController(IHttpService http)
        {
            _http = http;
        }

        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.SendEmail.Response>> SendEmail(CreateAR.Trellis.Messages.SendEmail.Request request)
        {   
            // Headers: [ Authorization ]
            return _http.Post<CreateAR.Trellis.Messages.SendEmail.Response>(
                "trellis://" + "/utilities/sendmail",
                request);
        }

        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.GettingStarted.Response>> GettingStarted(CreateAR.Trellis.Messages.GettingStarted.Request request)
        {   
            // Headers: [ Authorization ]
            return _http.Post<CreateAR.Trellis.Messages.GettingStarted.Response>(
                "trellis://" + "/getstarted",
                request);
        }
    }
}