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
            // Headers: [ Authorization, Content-Type ]
            return _http.Get<CreateAR.Trellis.Messages.GetMyApps.Response>(
                "trellis://" + "/editor/app");
        }
    
        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.GetApp.Response>> GetApp(string appId)
        {
            // Headers: [ Authorization, Content-Type ]
            return _http.Get<CreateAR.Trellis.Messages.GetApp.Response>(
                "trellis://" + string.Format("/editor/app/{0}", appId));
        }
    
        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.CreateApp.Response>> CreateApp(CreateAR.Trellis.Messages.CreateApp.Request request)
        {   
            // Headers: [ Authorization, Content-Type ]
            return _http.Post<CreateAR.Trellis.Messages.CreateApp.Response>(
                "trellis://" + "/editor/app",
                request);
        }

        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.DuplicateApp.Response>> DuplicateApp(CreateAR.Trellis.Messages.DuplicateApp.Request request)
        {   
            // Headers: [ Authorization, Content-Type ]
            return _http.Post<CreateAR.Trellis.Messages.DuplicateApp.Response>(
                "trellis://" + "/editor/app",
                request);
        }

        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.UpdateApp.Response>> UpdateApp(string appId, CreateAR.Trellis.Messages.UpdateApp.Request request)
        {   
            // Headers: [ Authorization, Content-Type ]
            return _http.Put<CreateAR.Trellis.Messages.UpdateApp.Response>(
                "trellis://" + string.Format("/editor/app/{0}", appId),
                request);
        }

        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.DeleteApp.Response>> DeleteApp(string appId)
        {
            // Headers: [ Authorization, Content-Type ]
            return _http.Delete<CreateAR.Trellis.Messages.DeleteApp.Response>(
                "trellis://" + string.Format("/editor/app/{0}", appId));
        }
        }
}