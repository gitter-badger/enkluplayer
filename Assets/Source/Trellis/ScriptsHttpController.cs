/**
 * NETIFY GENERATED CODE: DO NOT EDIT.
 */
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;

namespace CreateAR.Trellis.Messages
{
    public class ScriptsHttpController
    {
    
        private readonly IHttpService _http;
        
        public ScriptsHttpController(IHttpService http)
        {
            _http = http;
        }

        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.CreateScript.Response>> CreateScript(string appId, CreateAR.Trellis.Messages.CreateScript.Request request)
        {
            return _http.Post<CreateAR.Trellis.Messages.CreateScript.Response>(
                "trellis://" + string.Format("/editor/app/{0}/script", appId),
                request);
        }

        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.UpdateScript.Response>> UpdateScript(string appId, string scriptId, CreateAR.Trellis.Messages.UpdateScript.Request request)
        {
            return _http.Put<CreateAR.Trellis.Messages.UpdateScript.Response>(
                "trellis://" + string.Format("/editor/app/{0}/script/{1}", appId, scriptId),
                request);
        }

        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.GetAppScripts.Response>> GetAppScripts(string appId)
        {
            return _http.Get<CreateAR.Trellis.Messages.GetAppScripts.Response>(
                "trellis://" + string.Format("/editor/app/{0}/script", appId));
        }
    
        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.GetScript.Response>> GetScript(string appId, string scriptId)
        {
            return _http.Get<CreateAR.Trellis.Messages.GetScript.Response>(
                "trellis://" + string.Format("/editor/app/{0}/script/{1}", appId, scriptId));
        }
    
        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.DeleteScript.Response>> DeleteScript(string appId, string scriptId)
        {
            return _http.Delete<CreateAR.Trellis.Messages.DeleteScript.Response>(
                "trellis://" + string.Format("/editor/app/{0}/script/{1}", appId, scriptId));
        }
    }
}