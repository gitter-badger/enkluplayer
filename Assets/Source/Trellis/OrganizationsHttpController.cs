/**
 * NETIFY GENERATED CODE: DO NOT EDIT.
 */
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;

namespace CreateAR.Trellis.Messages
{
    public class OrganizationsHttpController
    {
    
        private readonly IHttpService _http;
        
        public OrganizationsHttpController(IHttpService http)
        {
            _http = http;
        }

        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.CreateOrganization.Response>> CreateOrganization(CreateAR.Trellis.Messages.CreateOrganization.Request request)
        {   
            // Headers: [ Authorization ]
            return _http.Post<CreateAR.Trellis.Messages.CreateOrganization.Response>(
                "trellis://" + "/org",
                request);
        }

        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.GetMyOrganizations.Response>> GetMyOrganizations()
        {
            // Headers: [ Authorization ]
            return _http.Get<CreateAR.Trellis.Messages.GetMyOrganizations.Response>(
                "trellis://" + "/org");
        }
    
        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.GetOrganization.Response>> GetOrganization(string organizationId)
        {
            // Headers: [ Authorization ]
            return _http.Get<CreateAR.Trellis.Messages.GetOrganization.Response>(
                "trellis://" + string.Format("/org/{0}", organizationId));
        }
    
        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.UpdateOrganization.Response>> UpdateOrganization(string organizationId, CreateAR.Trellis.Messages.UpdateOrganization.Request request)
        {   
            // Headers: [ Authorization ]
            return _http.Put<CreateAR.Trellis.Messages.UpdateOrganization.Response>(
                "trellis://" + string.Format("/org/{0}", organizationId),
                request);
        }

        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.DeleteOrganization.Response>> DeleteOrganization(string secondaryOrganizationId)
        {
            // Headers: [ Authorization ]
            return _http.Delete<CreateAR.Trellis.Messages.DeleteOrganization.Response>(
                "trellis://" + string.Format("/org/{0}", secondaryOrganizationId));
        }
        }
}