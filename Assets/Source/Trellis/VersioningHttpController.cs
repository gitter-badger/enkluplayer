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
            return _http.Get<CreateAR.Trellis.Messages.GetApiVersion.Response>(
                _http.Urls.Url("trellis://" + "/versions/api"));
        }
    
        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.GetHololensVersion.Response>> GetHololensVersion()
        {
            return _http.Get<CreateAR.Trellis.Messages.GetHololensVersion.Response>(
                _http.Urls.Url("trellis://" + "/versions/hololens"));
        }
    
        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.GetWebVersion.Response>> GetWebVersion()
        {
            return _http.Get<CreateAR.Trellis.Messages.GetWebVersion.Response>(
                _http.Urls.Url("trellis://" + "/versions/web"));
        }
    }
}