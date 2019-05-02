/**
 * NETIFY GENERATED CODE: DO NOT EDIT.
 */
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;

namespace CreateAR.Trellis.Messages
{
    public class AnchorsHttpController
    {
    
        private readonly IHttpService _http;
        
        public AnchorsHttpController(IHttpService http)
        {
            _http = http;
        }

        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.CreateAnchor.Response>> CreateAnchor(string appId, string sceneId, CreateAR.Trellis.Messages.CreateAnchor.Request request)
        {   
            // Headers: [ Authorization, Content-Type ]
            return _http.Post<CreateAR.Trellis.Messages.CreateAnchor.Response>(
                "trellis://" + string.Format("/editor/app/{0}/scene/{1}/anchor/foo", appId, sceneId),
                request);
        }

        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.UploadAnchor.Response>> UploadAnchor(string appId, string sceneId, string anchorId, CreateAR.Trellis.Messages.UploadAnchor.Request request)
        {   
            // Headers: [ Authorization, Content-Type ]
            return _http.Put<CreateAR.Trellis.Messages.UploadAnchor.Response>(
                "trellis://" + string.Format("/editor/app/{0}/scene/{1}/anchor/{2}", appId, sceneId, anchorId),
                request);
        }
    }
}