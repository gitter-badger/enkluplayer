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
            return _http.Post<CreateAR.Trellis.Messages.CreateOrganization.Response>(
                _http.Urls.Url("trellis://" + "/org"),
                request);
        }

        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.GetMyOrganizations.Response>> GetMyOrganizations()
        {
            return _http.Get<CreateAR.Trellis.Messages.GetMyOrganizations.Response>(
                _http.Urls.Url("trellis://" + "/org"));
        }
    
        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.GetOrganization.Response>> GetOrganization(string organizationId)
        {
            return _http.Get<CreateAR.Trellis.Messages.GetOrganization.Response>(
                _http.Urls.Url("trellis://" + string.Format("/org/{0}", organizationId)));
        }
    
        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.UpdateOrganization.Response>> UpdateOrganization(string organizationId, CreateAR.Trellis.Messages.UpdateOrganization.Request request)
        {
            return _http.Put<CreateAR.Trellis.Messages.UpdateOrganization.Response>(
                _http.Urls.Url("trellis://" + string.Format("/org/{0}", organizationId)),
                request);
        }

        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.DeleteOrganization.Response>> DeleteOrganization(string secondaryOrganizationId)
        {
            return _http.Delete<CreateAR.Trellis.Messages.DeleteOrganization.Response>(
                _http.Urls.Url("trellis://" + string.Format("/org/{0}", secondaryOrganizationId)));
        }
    }
}