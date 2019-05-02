/**
 * NETIFY GENERATED CODE: DO NOT EDIT.
 */
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;

namespace CreateAR.Trellis.Messages
{
    public class AccountHttpController
    {
    
        private readonly IHttpService _http;
        
        public AccountHttpController(IHttpService http)
        {
            _http = http;
        }

        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.CreateSubscription.Response>> CreateSubscription(CreateAR.Trellis.Messages.CreateSubscription.Request request)
        {   
            // Headers: [ Authorization, Content-Type ]
            return _http.Post<CreateAR.Trellis.Messages.CreateSubscription.Response>(
                "trellis://" + "/account/subscription",
                request);
        }

        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.UpdateSubscription.Response>> UpdateSubscription(CreateAR.Trellis.Messages.UpdateSubscription.Request request)
        {   
            // Headers: [ Authorization, Content-Type ]
            return _http.Put<CreateAR.Trellis.Messages.UpdateSubscription.Response>(
                "trellis://" + "/account/subscription",
                request);
        }
    }
}