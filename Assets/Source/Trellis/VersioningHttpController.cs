/**
 * NETIFY GENERATED CODE: DO NOT EDIT.
 */
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;

namespace CreateAR.Trellis.Messages
{
    public class VersioningHttpController
    {
    
        private readonly IHttpService _http;
        
        public VersioningHttpController(IHttpService http)
        {
            _http = http;
        }

        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.GetApiVersion.Response>> GetApiVersion()
        {
            // Headers: [  ]
            return _http.Get<CreateAR.Trellis.Messages.GetApiVersion.Response>(
                "trellis://" + "/versions/api");
        }
    
        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.GetHololensVersion.Response>> GetHololensVersion()
        {
            // Headers: [  ]
            return _http.Get<CreateAR.Trellis.Messages.GetHololensVersion.Response>(
                "trellis://" + "/versions/hololens");
        }
    
        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.GetWebVersion.Response>> GetWebVersion()
        {
            // Headers: [  ]
            return _http.Get<CreateAR.Trellis.Messages.GetWebVersion.Response>(
                "trellis://" + "/versions/web");
        }
        }
}