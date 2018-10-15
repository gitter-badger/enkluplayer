/**
 * NETIFY GENERATED CODE: DO NOT EDIT.
 */
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;

namespace CreateAR.Trellis.Messages
{
    public class ScenesHttpController
    {
    
        private readonly IHttpService _http;
        
        public ScenesHttpController(IHttpService http)
        {
            _http = http;
        }

        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.CreateScene.Response>> CreateScene(string appId, CreateAR.Trellis.Messages.CreateScene.Request request)
        {
            return _http.Post<CreateAR.Trellis.Messages.CreateScene.Response>(
                _http.Urls.Url("trellis://" + string.Format("/editor/app/{0}/scene", appId)),
                request);
        }

        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.GetScene.Response>> GetScene(string appId, string sceneId)
        {
            return _http.Get<CreateAR.Trellis.Messages.GetScene.Response>(
                _http.Urls.Url("trellis://" + string.Format("/editor/app/{0}/scene/{1}", appId, sceneId)));
        }
    
        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.CreateSceneElement.Response>> CreateSceneElement(string appId, string sceneId, CreateAR.Trellis.Messages.CreateSceneElement.Request request)
        {
            return _http.Put<CreateAR.Trellis.Messages.CreateSceneElement.Response>(
                _http.Urls.Url("trellis://" + string.Format("/editor/app/{0}/scene/{1}", appId, sceneId)),
                request);
        }

        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.UpdateSceneElementString.Response>> UpdateSceneElementString(string appId, string sceneId, CreateAR.Trellis.Messages.UpdateSceneElementString.Request request)
        {
            return _http.Put<CreateAR.Trellis.Messages.UpdateSceneElementString.Response>(
                _http.Urls.Url("trellis://" + string.Format("/editor/app/{0}/scene/{1}", appId, sceneId)),
                request);
        }

        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.UpdateSceneElementInt.Response>> UpdateSceneElementInt(string appId, string sceneId, CreateAR.Trellis.Messages.UpdateSceneElementInt.Request request)
        {
            return _http.Put<CreateAR.Trellis.Messages.UpdateSceneElementInt.Response>(
                _http.Urls.Url("trellis://" + string.Format("/editor/app/{0}/scene/{1}", appId, sceneId)),
                request);
        }

        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.UpdateSceneElementFloat.Response>> UpdateSceneElementFloat(string appId, string sceneId, CreateAR.Trellis.Messages.UpdateSceneElementFloat.Request request)
        {
            return _http.Put<CreateAR.Trellis.Messages.UpdateSceneElementFloat.Response>(
                _http.Urls.Url("trellis://" + string.Format("/editor/app/{0}/scene/{1}", appId, sceneId)),
                request);
        }

        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.UpdateSceneElementBool.Response>> UpdateSceneElementBool(string appId, string sceneId, CreateAR.Trellis.Messages.UpdateSceneElementBool.Request request)
        {
            return _http.Put<CreateAR.Trellis.Messages.UpdateSceneElementBool.Response>(
                _http.Urls.Url("trellis://" + string.Format("/editor/app/{0}/scene/{1}", appId, sceneId)),
                request);
        }

        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.UpdateSceneElementVec3.Response>> UpdateSceneElementVec3(string appId, string sceneId, CreateAR.Trellis.Messages.UpdateSceneElementVec3.Request request)
        {
            return _http.Put<CreateAR.Trellis.Messages.UpdateSceneElementVec3.Response>(
                _http.Urls.Url("trellis://" + string.Format("/editor/app/{0}/scene/{1}", appId, sceneId)),
                request);
        }

        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.UpdateSceneElementCol4.Response>> UpdateSceneElementCol4(string appId, string sceneId, CreateAR.Trellis.Messages.UpdateSceneElementCol4.Request request)
        {
            return _http.Put<CreateAR.Trellis.Messages.UpdateSceneElementCol4.Response>(
                _http.Urls.Url("trellis://" + string.Format("/editor/app/{0}/scene/{1}", appId, sceneId)),
                request);
        }

        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.DeleteSceneElement.Response>> DeleteSceneElement(string appId, string sceneId, CreateAR.Trellis.Messages.DeleteSceneElement.Request request)
        {
            return _http.Put<CreateAR.Trellis.Messages.DeleteSceneElement.Response>(
                _http.Urls.Url("trellis://" + string.Format("/editor/app/{0}/scene/{1}", appId, sceneId)),
                request);
        }
}
}