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
            // Headers: [ Authorization ]
            return _http.Get<CreateAR.Trellis.Messages.GetAllKvs.Response>(
                "trellis://" + "/kv");
        }
    
        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.GetAllKvHeaders.Response>> GetAllKvHeaders()
        {
            // Headers: [ Authorization ]
            return _http.Get<CreateAR.Trellis.Messages.GetAllKvHeaders.Response>(
                "trellis://" + "/kv?headers=true");
        }
    
        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.CreateKv.Response>> CreateKv(CreateAR.Trellis.Messages.CreateKv.Request request)
        {   
            // Headers: [ Authorization, Content-Type ]
            return _http.Post<CreateAR.Trellis.Messages.CreateKv.Response>(
                "trellis://" + "/kv",
                request);
        }

        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.GetKv.Response>> GetKv(string kvid)
        {
            // Headers: [ Authorization ]
            return _http.Get<CreateAR.Trellis.Messages.GetKv.Response>(
                "trellis://" + string.Format("/kv/{0}", kvid));
        }
    
        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.DeleteKv.Response>> DeleteKv(string kvid)
        {
            // Headers: [ Authorization ]
            return _http.Delete<CreateAR.Trellis.Messages.DeleteKv.Response>(
                "trellis://" + string.Format("/kv/{0}", kvid));
        }
    
        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.UpdateKv.Response>> UpdateKv(string kvid, CreateAR.Trellis.Messages.UpdateKv.Request request)
        {   
            // Headers: [ Authorization, Content-Type ]
            return _http.Put<CreateAR.Trellis.Messages.UpdateKv.Response>(
                "trellis://" + string.Format("/kv/{0}", kvid),
                request);
        }

        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.GetAllKvsBytag.Response>> GetAllKvsBytag(string kvTag)
        {
            // Headers: [ Authorization ]
            return _http.Get<CreateAR.Trellis.Messages.GetAllKvsBytag.Response>(
                "trellis://" + string.Format("/kv?tags={0}", kvTag));
        }
        }
}