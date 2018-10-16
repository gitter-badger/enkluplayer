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
            return _http.Get<CreateAR.Trellis.Messages.GetMyNeighborhoods.Response>(
                _http.Urls.Url("trellis://" + "/neighborhood"));
        }
    
        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.GetNeighborhood.Response>> GetNeighborhood(string neighborhoodId)
        {
            return _http.Get<CreateAR.Trellis.Messages.GetNeighborhood.Response>(
                _http.Urls.Url("trellis://" + string.Format("/neighborhood/{0}", neighborhoodId)));
        }
    
        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.JoinNeighborhood.Response>> JoinNeighborhood(string neighborhoodId, CreateAR.Trellis.Messages.JoinNeighborhood.Request request)
        {
            return _http.Post<CreateAR.Trellis.Messages.JoinNeighborhood.Response>(
                _http.Urls.Url("trellis://" + string.Format("/neighborhood/{0}/neighbor", neighborhoodId)),
                request);
        }

        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.GetNeighbors.Response>> GetNeighbors(string neighborhoodId)
        {
            return _http.Get<CreateAR.Trellis.Messages.GetNeighbors.Response>(
                _http.Urls.Url("trellis://" + string.Format("/neighborhood/{0}/neighbor", neighborhoodId)));
        }
    
        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.GetNeighbor.Response>> GetNeighbor(string neighborhoodId, string neighborId)
        {
            return _http.Get<CreateAR.Trellis.Messages.GetNeighbor.Response>(
                _http.Urls.Url("trellis://" + string.Format("/neighborhood/{0}/neighbor/{1}", neighborhoodId, neighborId)));
        }
    
        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.GetNeighborhoodKvs.Response>> GetNeighborhoodKvs(string neighborhoodId)
        {
            return _http.Get<CreateAR.Trellis.Messages.GetNeighborhoodKvs.Response>(
                _http.Urls.Url("trellis://" + string.Format("/neighborhood/{0}/kv", neighborhoodId)));
        }
    
        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.GetNeighborhoodKv.Response>> GetNeighborhoodKv(string neighborhoodId, string neighborKvId)
        {
            return _http.Get<CreateAR.Trellis.Messages.GetNeighborhoodKv.Response>(
                _http.Urls.Url("trellis://" + string.Format("/neighborhood/{0}/kv/{1}", neighborhoodId, neighborKvId)));
        }
    
        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.CreateNeighborhoodKv.Response>> CreateNeighborhoodKv(string neighborhoodId, CreateAR.Trellis.Messages.CreateNeighborhoodKv.Request request)
        {
            return _http.Post<CreateAR.Trellis.Messages.CreateNeighborhoodKv.Response>(
                _http.Urls.Url("trellis://" + string.Format("/neighborhood/{0}/kv", neighborhoodId)),
                request);
        }

        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.UpdateNeighborhoodKv.Response>> UpdateNeighborhoodKv(string neighborhoodId, string neighborKvId, CreateAR.Trellis.Messages.UpdateNeighborhoodKv.Request request)
        {
            return _http.Put<CreateAR.Trellis.Messages.UpdateNeighborhoodKv.Response>(
                _http.Urls.Url("trellis://" + string.Format("/neighborhood/{0}/kv/{1}", neighborhoodId, neighborKvId)),
                request);
        }

        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.DeleteNeighborhoodKv.Response>> DeleteNeighborhoodKv(string neighborhoodId, string neighborKvId)
        {
            return _http.Delete<CreateAR.Trellis.Messages.DeleteNeighborhoodKv.Response>(
                _http.Urls.Url("trellis://" + string.Format("/neighborhood/{0}/kv/{1}", neighborhoodId, neighborKvId)));
        }
    
        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.CreateNeighborhood.Response>> CreateNeighborhood(CreateAR.Trellis.Messages.CreateNeighborhood.Request request)
        {
            return _http.Post<CreateAR.Trellis.Messages.CreateNeighborhood.Response>(
                _http.Urls.Url("trellis://" + "/neighborhood"),
                request);
        }

        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.DeleteNeighborhood.Response>> DeleteNeighborhood(string neighborhoodId)
        {
            return _http.Delete<CreateAR.Trellis.Messages.DeleteNeighborhood.Response>(
                _http.Urls.Url("trellis://" + string.Format("/neighborhood/{0}", neighborhoodId)));
        }
    }
}