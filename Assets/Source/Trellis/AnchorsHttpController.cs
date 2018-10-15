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

        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.UploadAnchor.Response>> UploadAnchor(string appId, string sceneId, string anchorId, CreateAR.Trellis.Messages.UploadAnchor.Request request)
        {
            return _http.Put<CreateAR.Trellis.Messages.UploadAnchor.Response>(
                _http.Urls.Url("trellis://" + string.Format("/editor/app/{0}/scene/{1}/anchor/{2}", appId, sceneId, anchorId)),
                request);
        }
}
}