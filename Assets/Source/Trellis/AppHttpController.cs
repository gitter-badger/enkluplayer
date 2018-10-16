/**
 * NETIFY GENERATED CODE: DO NOT EDIT.
 */
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;

namespace CreateAR.Trellis.Messages
{
    public class AppHttpController
    {
    
        private readonly IHttpService _http;
        
        public AppHttpController(IHttpService http)
        {
            _http = http;
        }

        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.GetMyApps.Response>> GetMyApps()
        {
            return _http.Get<CreateAR.Trellis.Messages.GetMyApps.Response>(
                _http.Urls.Url("trellis://" + "/editor/app"));
        }
    
        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.GetApp.Response>> GetApp(string appId)
        {
            return _http.Get<CreateAR.Trellis.Messages.GetApp.Response>(
                _http.Urls.Url("trellis://" + string.Format("/editor/app/{0}", appId)));
        }
    
        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.CreateApp.Response>> CreateApp(CreateAR.Trellis.Messages.CreateApp.Request request)
        {
            return _http.Post<CreateAR.Trellis.Messages.CreateApp.Response>(
                _http.Urls.Url("trellis://" + "/editor/app"),
                request);
        }

        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.UpdateApp.Response>> UpdateApp(string appId, CreateAR.Trellis.Messages.UpdateApp.Request request)
        {
            return _http.Put<CreateAR.Trellis.Messages.UpdateApp.Response>(
                _http.Urls.Url("trellis://" + string.Format("/editor/app/{0}", appId)),
                request);
        }

        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.DeleteApp.Response>> DeleteApp(string appId)
        {
            return _http.Delete<CreateAR.Trellis.Messages.DeleteApp.Response>(
                _http.Urls.Url("trellis://" + string.Format("/editor/app/{0}", appId)));
        }
    }
}