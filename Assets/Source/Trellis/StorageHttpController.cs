/**
 * NETIFY GENERATED CODE: DO NOT EDIT.
 */
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;

namespace CreateAR.Trellis.Messages
{
    public class StorageHttpController
    {
    
        private readonly IHttpService _http;
        
        public StorageHttpController(IHttpService http)
        {
            _http = http;
        }

        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.GetAllKvs.Response>> GetAllKvs()
        {
            return _http.Get<CreateAR.Trellis.Messages.GetAllKvs.Response>(
                _http.Urls.Url("trellis://" + "/kv"));
        }
    
        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.CreateKv.Response>> CreateKv(CreateAR.Trellis.Messages.CreateKv.Request request)
        {
            return _http.Post<CreateAR.Trellis.Messages.CreateKv.Response>(
                _http.Urls.Url("trellis://" + "/kv"),
                request);
        }

        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.GetKv.Response>> GetKv(string kvid)
        {
            return _http.Get<CreateAR.Trellis.Messages.GetKv.Response>(
                _http.Urls.Url("trellis://" + string.Format("/kv/{0}", kvid)));
        }
    
        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.DeleteKv.Response>> DeleteKv(string kvid)
        {
            return _http.Delete<CreateAR.Trellis.Messages.DeleteKv.Response>(
                _http.Urls.Url("trellis://" + string.Format("/kv/{0}", kvid)));
        }
    
        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.UpdateKv.Response>> UpdateKv(string kvid, CreateAR.Trellis.Messages.UpdateKv.Request request)
        {
            return _http.Put<CreateAR.Trellis.Messages.UpdateKv.Response>(
                _http.Urls.Url("trellis://" + string.Format("/kv/{0}", kvid)),
                request);
        }

        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.GetAllKvsBytag.Response>> GetAllKvsBytag(string kvTag)
        {
            return _http.Get<CreateAR.Trellis.Messages.GetAllKvsBytag.Response>(
                _http.Urls.Url("trellis://" + string.Format("/kv?tags={0}", kvTag)));
        }
    }
}