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

        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.CreateSnap.Response>> CreateSnap(string organizationId, string instanceId, CreateAR.Trellis.Messages.CreateSnap.Request request)
        {   
            // Headers: [ Authorization, Content-Type ]
            return _http.Post<CreateAR.Trellis.Messages.CreateSnap.Response>(
                "trellis://" + string.Format("/org/{0}/snap/{1}", organizationId, instanceId),
                request);
        }

        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.TriggerSnap.Response>> TriggerSnap(string organizationId, string instanceId, CreateAR.Trellis.Messages.TriggerSnap.Request request)
        {   
            // Headers: [ Authorization, Content-Type ]
            return _http.Put<CreateAR.Trellis.Messages.TriggerSnap.Response>(
                "trellis://" + string.Format("/org/{0}/snap/{1}", organizationId, instanceId),
                request);
        }
    }
}