/**
 * NETIFY GENERATED CODE: DO NOT EDIT.
 */
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;

namespace CreateAR.Trellis.Messages
{
    public class AssetLibrariesHttpController
    {
    
        private readonly IHttpService _http;
        
        public AssetLibrariesHttpController(IHttpService http)
        {
            _http = http;
        }

        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.GetAnAsset.Response>> GetAnAsset(string assetid)
        {
            // Headers: [ Authorization ]
            return _http.Get<CreateAR.Trellis.Messages.GetAnAsset.Response>(
                "trellis://" + string.Format("/asset/{0}", assetid));
        }
    
        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.GetAppAssets.Response>> GetAppAssets(string appId)
        {
            // Headers: [ Authorization, Content-Type ]
            return _http.Get<CreateAR.Trellis.Messages.GetAppAssets.Response>(
                "trellis://" + string.Format("/editor/app/{0}/library", appId));
        }
    
        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.ShareAssetWithApp.Response>> ShareAssetWithApp(string appId, CreateAR.Trellis.Messages.ShareAssetWithApp.Request request)
        {   
            // Headers: [ Authorization, Content-Type ]
            return _http.Post<CreateAR.Trellis.Messages.ShareAssetWithApp.Response>(
                "trellis://" + string.Format("/editor/app/{0}/library", appId),
                request);
        }

        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.GetPersonalAssets.Response>> GetPersonalAssets(string userid)
        {
            // Headers: [ Authorization, Content-Type ]
            return _http.Get<CreateAR.Trellis.Messages.GetPersonalAssets.Response>(
                "trellis://" + string.Format("/user/{0}/library", userid));
        }
    
        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.GetPublicAssets.Response>> GetPublicAssets(string assetTag)
        {
            // Headers: [ Authorization, Content-Type ]
            return _http.Get<CreateAR.Trellis.Messages.GetPublicAssets.Response>(
                "trellis://" + string.Format("/library?tag={0}", assetTag));
        }
    
        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.UpdateAnAsset.Response>> UpdateAnAsset(string assetid, CreateAR.Trellis.Messages.UpdateAnAsset.Request request)
        {   
            // Headers: [ Authorization ]
            return _http.Put<CreateAR.Trellis.Messages.UpdateAnAsset.Response>(
                "trellis://" + string.Format("/asset/{0}", assetid),
                request);
        }

        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.DeleteMyAsset.Response>> DeleteMyAsset(string assetid)
        {
            // Headers: [ Authorization ]
            return _http.Delete<CreateAR.Trellis.Messages.DeleteMyAsset.Response>(
                "trellis://" + string.Format("/asset/{0}", assetid));
        }
        }
}