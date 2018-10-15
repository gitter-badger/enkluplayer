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
            return _http.Get<CreateAR.Trellis.Messages.GetAnAsset.Response>(
                _http.Urls.Url("trellis://" + string.Format("/asset/{0}", assetid)));
        }
    
        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.GetAppAssets.Response>> GetAppAssets(string appId)
        {
            return _http.Get<CreateAR.Trellis.Messages.GetAppAssets.Response>(
                _http.Urls.Url("trellis://" + string.Format("/editor/app/{0}/library", appId)));
        }
    
        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.ShareAssetWithApp.Response>> ShareAssetWithApp(string appId, CreateAR.Trellis.Messages.ShareAssetWithApp.Request request)
        {
            return _http.Post<CreateAR.Trellis.Messages.ShareAssetWithApp.Response>(
                _http.Urls.Url("trellis://" + string.Format("/editor/app/{0}/library", appId)),
                request);
        }

        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.GetPersonalAssets.Response>> GetPersonalAssets(string userid)
        {
            return _http.Get<CreateAR.Trellis.Messages.GetPersonalAssets.Response>(
                _http.Urls.Url("trellis://" + string.Format("/user/{0}/library", userid)));
        }
    
        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.GetPublicAssets.Response>> GetPublicAssets(string assetTag)
        {
            return _http.Get<CreateAR.Trellis.Messages.GetPublicAssets.Response>(
                _http.Urls.Url("trellis://" + string.Format("/library?tag={0}", assetTag)));
        }
    
        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.UpdateAnAsset.Response>> UpdateAnAsset(string assetid, CreateAR.Trellis.Messages.UpdateAnAsset.Request request)
        {
            return _http.Put<CreateAR.Trellis.Messages.UpdateAnAsset.Response>(
                _http.Urls.Url("trellis://" + string.Format("/asset/{0}", assetid)),
                request);
        }

        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.DeleteMyAsset.Response>> DeleteMyAsset(string assetid)
        {
            return _http.Delete<CreateAR.Trellis.Messages.DeleteMyAsset.Response>(
                _http.Urls.Url("trellis://" + string.Format("/asset/{0}", assetid)));
        }
    }
}