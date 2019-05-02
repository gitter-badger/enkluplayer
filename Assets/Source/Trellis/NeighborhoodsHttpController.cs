/**
 * NETIFY GENERATED CODE: DO NOT EDIT.
 */
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;

namespace CreateAR.Trellis.Messages
{
    public class NeighborhoodsHttpController
    {
    
        private readonly IHttpService _http;
        
        public NeighborhoodsHttpController(IHttpService http)
        {
            _http = http;
        }

        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.GetMyNeighborhoods.Response>> GetMyNeighborhoods()
        {
            // Headers: [ Authorization, Content-Type ]
            return _http.Get<CreateAR.Trellis.Messages.GetMyNeighborhoods.Response>(
                "trellis://" + "/neighborhood");
        }
    
        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.GetNeighborhood.Response>> GetNeighborhood(string neighborhoodId)
        {
            // Headers: [ Authorization, Content-Type ]
            return _http.Get<CreateAR.Trellis.Messages.GetNeighborhood.Response>(
                "trellis://" + string.Format("/neighborhood/{0}", neighborhoodId));
        }
    
        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.JoinNeighborhood.Response>> JoinNeighborhood(string neighborhoodId, CreateAR.Trellis.Messages.JoinNeighborhood.Request request)
        {   
            // Headers: [ Authorization, Content-Type ]
            return _http.Post<CreateAR.Trellis.Messages.JoinNeighborhood.Response>(
                "trellis://" + string.Format("/neighborhood/{0}/neighbor", neighborhoodId),
                request);
        }

        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.GetNeighbors.Response>> GetNeighbors(string neighborhoodId)
        {
            // Headers: [ Authorization, Content-Type ]
            return _http.Get<CreateAR.Trellis.Messages.GetNeighbors.Response>(
                "trellis://" + string.Format("/neighborhood/{0}/neighbor", neighborhoodId));
        }
    
        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.GetNeighbor.Response>> GetNeighbor(string neighborhoodId, string neighborId)
        {
            // Headers: [ Authorization, Content-Type ]
            return _http.Get<CreateAR.Trellis.Messages.GetNeighbor.Response>(
                "trellis://" + string.Format("/neighborhood/{0}/neighbor/{1}", neighborhoodId, neighborId));
        }
    
        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.GetNeighborhoodKvs.Response>> GetNeighborhoodKvs(string neighborhoodId)
        {
            // Headers: [ Authorization, Content-Type ]
            return _http.Get<CreateAR.Trellis.Messages.GetNeighborhoodKvs.Response>(
                "trellis://" + string.Format("/neighborhood/{0}/kv", neighborhoodId));
        }
    
        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.GetNeighborhoodKv.Response>> GetNeighborhoodKv(string neighborhoodId, string neighborKvId)
        {
            // Headers: [ Authorization, Content-Type ]
            return _http.Get<CreateAR.Trellis.Messages.GetNeighborhoodKv.Response>(
                "trellis://" + string.Format("/neighborhood/{0}/kv/{1}", neighborhoodId, neighborKvId));
        }
    
        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.CreateNeighborhoodKv.Response>> CreateNeighborhoodKv(string neighborhoodId, CreateAR.Trellis.Messages.CreateNeighborhoodKv.Request request)
        {   
            // Headers: [ Authorization, Content-Type ]
            return _http.Post<CreateAR.Trellis.Messages.CreateNeighborhoodKv.Response>(
                "trellis://" + string.Format("/neighborhood/{0}/kv", neighborhoodId),
                request);
        }

        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.UpdateNeighborhoodKv.Response>> UpdateNeighborhoodKv(string neighborhoodId, string neighborKvId, CreateAR.Trellis.Messages.UpdateNeighborhoodKv.Request request)
        {   
            // Headers: [ Authorization, Content-Type ]
            return _http.Put<CreateAR.Trellis.Messages.UpdateNeighborhoodKv.Response>(
                "trellis://" + string.Format("/neighborhood/{0}/kv/{1}", neighborhoodId, neighborKvId),
                request);
        }

        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.DeleteNeighborhoodKv.Response>> DeleteNeighborhoodKv(string neighborhoodId, string neighborKvId)
        {
            // Headers: [ Authorization, Content-Type ]
            return _http.Delete<CreateAR.Trellis.Messages.DeleteNeighborhoodKv.Response>(
                "trellis://" + string.Format("/neighborhood/{0}/kv/{1}", neighborhoodId, neighborKvId));
        }
    
        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.CreateNeighborhood.Response>> CreateNeighborhood(CreateAR.Trellis.Messages.CreateNeighborhood.Request request)
        {   
            // Headers: [ Authorization, Content-Type ]
            return _http.Post<CreateAR.Trellis.Messages.CreateNeighborhood.Response>(
                "trellis://" + "/neighborhood",
                request);
        }

        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.DeleteNeighborhood.Response>> DeleteNeighborhood(string neighborhoodId)
        {
            // Headers: [ Authorization, Content-Type ]
            return _http.Delete<CreateAR.Trellis.Messages.DeleteNeighborhood.Response>(
                "trellis://" + string.Format("/neighborhood/{0}", neighborhoodId));
        }
        }
}