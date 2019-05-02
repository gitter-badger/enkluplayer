/**
 * NETIFY GENERATED CODE: DO NOT EDIT.
 */
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;

namespace CreateAR.Trellis.Messages
{
    public class HoloAuthHttpController
    {
    
        private readonly IHttpService _http;
        
        public HoloAuthHttpController(IHttpService http)
        {
            _http = http;
        }

        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.HoloAuthorize.Response>> HoloAuthorize(CreateAR.Trellis.Messages.HoloAuthorize.Request request)
        {   
            // Headers: [ Authorization ]
            return _http.Post<CreateAR.Trellis.Messages.HoloAuthorize.Response>(
                "trellis://" + "/holo/authorize",
                request);
        }

        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.HoloSignin.Response>> HoloSignin(CreateAR.Trellis.Messages.HoloSignin.Request request)
        {   
            // Headers: [ Authorization ]
            return _http.Post<CreateAR.Trellis.Messages.HoloSignin.Response>(
                "trellis://" + "/holo/signin",
                request);
        }
    }
}