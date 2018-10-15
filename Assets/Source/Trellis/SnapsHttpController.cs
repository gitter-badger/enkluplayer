/**
 * NETIFY GENERATED CODE: DO NOT EDIT.
 */
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;

namespace CreateAR.Trellis.Messages
{
    public class SnapsHttpController
    {
    
        private readonly IHttpService _http;
        
        public SnapsHttpController(IHttpService http)
        {
            _http = http;
        }

        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.GetAllSnaps.Response>> GetAllSnaps(string organizationId)
        {
            return _http.Get<CreateAR.Trellis.Messages.GetAllSnaps.Response>(
                _http.Urls.Url("trellis://" + string.Format("/org/{0}/snap", organizationId)));
        }
    
        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.CreateSnap.Response>> CreateSnap(string organizationId, string instanceId, CreateAR.Trellis.Messages.CreateSnap.Request request)
        {
            return _http.Post<CreateAR.Trellis.Messages.CreateSnap.Response>(
                _http.Urls.Url("trellis://" + string.Format("/org/{0}/snap/{1}", organizationId, instanceId)),
                request);
        }

        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.GetSnap.Response>> GetSnap(string organizationId, string instanceId, string snapId)
        {
            return _http.Get<CreateAR.Trellis.Messages.GetSnap.Response>(
                _http.Urls.Url("trellis://" + string.Format("/org/{0}/snap/{1}/{2}", organizationId, instanceId, snapId)));
        }
    
        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.SendSnap.Response>> SendSnap(string organizationId, string instanceId, string snapId, CreateAR.Trellis.Messages.SendSnap.Request request)
        {
            return _http.Put<CreateAR.Trellis.Messages.SendSnap.Response>(
                _http.Urls.Url("trellis://" + string.Format("/org/{0}/snap/{1}/{2}", organizationId, instanceId, snapId)),
                request);
        }

        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.TriggerSnap.Response>> TriggerSnap(string organizationId, string instanceId, CreateAR.Trellis.Messages.TriggerSnap.Request request)
        {
            return _http.Put<CreateAR.Trellis.Messages.TriggerSnap.Response>(
                _http.Urls.Url("trellis://" + string.Format("/org/{0}/snap/{1}", organizationId, instanceId)),
                request);
        }
}
}