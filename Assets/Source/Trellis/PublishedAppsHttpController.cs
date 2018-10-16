/**
 * NETIFY GENERATED CODE: DO NOT EDIT.
 */
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;

namespace CreateAR.Trellis.Messages
{
    public class PublishedAppsHttpController
    {
    
        private readonly IHttpService _http;
        
        public PublishedAppsHttpController(IHttpService http)
        {
            _http = http;
        }

        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.GetPublishedApp.Response>> GetPublishedApp(string publicAppId)
        {
            return _http.Get<CreateAR.Trellis.Messages.GetPublishedApp.Response>(
                _http.Urls.Url("trellis://" + string.Format("/app/{0}", publicAppId)));
        }
    
        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.GetPublishedScene.Response>> GetPublishedScene(string publicAppId, string publicSceneId)
        {
            return _http.Get<CreateAR.Trellis.Messages.GetPublishedScene.Response>(
                _http.Urls.Url("trellis://" + string.Format("/app/{0}/scene/{1}", publicAppId, publicSceneId)));
        }
    
        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.GetPublishedAssets.Response>> GetPublishedAssets(string publicAppId)
        {
            return _http.Get<CreateAR.Trellis.Messages.GetPublishedAssets.Response>(
                _http.Urls.Url("trellis://" + string.Format("/app/{0}/asset", publicAppId)));
        }
    
        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.GetPublishedAsset.Response>> GetPublishedAsset(string publicAppId, string publicAssetId)
        {
            return _http.Get<CreateAR.Trellis.Messages.GetPublishedAsset.Response>(
                _http.Urls.Url("trellis://" + string.Format("/app/{0}/asset/{1}", publicAppId, publicAssetId)));
        }
    
        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.GetPublishedAppScripts.Response>> GetPublishedAppScripts(string publicAppId)
        {
            return _http.Get<CreateAR.Trellis.Messages.GetPublishedAppScripts.Response>(
                _http.Urls.Url("trellis://" + string.Format("/app/{0}/script", publicAppId)));
        }
    
        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.GetPublishedScript.Response>> GetPublishedScript(string publicAppId, string publicScriptId)
        {
            return _http.Get<CreateAR.Trellis.Messages.GetPublishedScript.Response>(
                _http.Urls.Url("trellis://" + string.Format("/app/{0}/script/{1}", publicAppId, publicScriptId)));
        }
    
        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.SearchPublishedApps.Response>> SearchPublishedApps(string publicAppQuery)
        {
            return _http.Get<CreateAR.Trellis.Messages.SearchPublishedApps.Response>(
                _http.Urls.Url("trellis://" + string.Format("/app?query={0}", publicAppQuery)));
        }
    }
}