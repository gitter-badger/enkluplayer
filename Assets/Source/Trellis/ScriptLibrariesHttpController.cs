/**
 * NETIFY GENERATED CODE: DO NOT EDIT.
 */
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;

namespace CreateAR.Trellis.Messages
{
    public class ScriptLibrariesHttpController
    {
    
        private readonly IHttpService _http;
        
        public ScriptLibrariesHttpController(IHttpService http)
        {
            _http = http;
        }

        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.GetAppScripts.Response>> GetAppScripts(string appId)
        {
            // Headers: [ Authorization ]
            return _http.Get<CreateAR.Trellis.Messages.GetAppScripts.Response>(
                "trellis://" + string.Format("/app/{0}/script-library", appId));
        }
    
        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.GetPersonalScripts.Response>> GetPersonalScripts(string userid)
        {
            // Headers: [ Authorization ]
            return _http.Get<CreateAR.Trellis.Messages.GetPersonalScripts.Response>(
                "trellis://" + string.Format("/user/{0}/script-library", userid));
        }
    
        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.GetPublicScripts.Response>> GetPublicScripts(string scriptTag)
        {
            // Headers: [ Authorization ]
            return _http.Get<CreateAR.Trellis.Messages.GetPublicScripts.Response>(
                "trellis://" + string.Format("/script-library?tag={0}", scriptTag));
        }
    
        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.GetScript.Response>> GetScript(string scriptId)
        {
            // Headers: [ Authorization ]
            return _http.Get<CreateAR.Trellis.Messages.GetScript.Response>(
                "trellis://" + string.Format("/script/{0}", scriptId));
        }
    
        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.CreateScript.Response>> CreateScript(CreateAR.Trellis.Messages.CreateScript.Request request)
        {   
            // Headers: [ Authorization, Content-Type ]
            return _http.Post<CreateAR.Trellis.Messages.CreateScript.Response>(
                "trellis://" + "/script",
                request);
        }

        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.UpdateScript.Response>> UpdateScript(string scriptId, CreateAR.Trellis.Messages.UpdateScript.Request request)
        {   
            // Headers: [ Authorization, Content-Type ]
            return _http.Put<CreateAR.Trellis.Messages.UpdateScript.Response>(
                "trellis://" + string.Format("/script/{0}", scriptId),
                request);
        }

        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.DeleteScript.Response>> DeleteScript(string scriptId)
        {
            // Headers: [ Authorization ]
            return _http.Delete<CreateAR.Trellis.Messages.DeleteScript.Response>(
                "trellis://" + string.Format("/script/{0}", scriptId));
        }
    
        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.ShareScriptWithApp.Response>> ShareScriptWithApp(string appId, CreateAR.Trellis.Messages.ShareScriptWithApp.Request request)
        {   
            // Headers: [ Authorization, Content-Type ]
            return _http.Post<CreateAR.Trellis.Messages.ShareScriptWithApp.Response>(
                "trellis://" + string.Format("/app/{0}/script-library", appId),
                request);
        }
    }
}