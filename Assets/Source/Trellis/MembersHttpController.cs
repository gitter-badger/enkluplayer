/**
 * NETIFY GENERATED CODE: DO NOT EDIT.
 */
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;

namespace CreateAR.Trellis.Messages
{
    public class MembersHttpController
    {
    
        private readonly IHttpService _http;
        
        public MembersHttpController(IHttpService http)
        {
            _http = http;
        }

        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.CreateMember.Response>> CreateMember(string organizationId, CreateAR.Trellis.Messages.CreateMember.Request request)
        {
            return _http.Post<CreateAR.Trellis.Messages.CreateMember.Response>(
                _http.Urls.Url("trellis://" + string.Format("/org/{0}/member", organizationId)),
                request);
        }

        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.GetAllOrganizationMembers.Response>> GetAllOrganizationMembers(string organizationId)
        {
            return _http.Get<CreateAR.Trellis.Messages.GetAllOrganizationMembers.Response>(
                _http.Urls.Url("trellis://" + string.Format("/org/{0}/member", organizationId)));
        }
    
        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.GetOrganizationMember.Response>> GetOrganizationMember(string organizationId, string orgMemberId)
        {
            return _http.Get<CreateAR.Trellis.Messages.GetOrganizationMember.Response>(
                _http.Urls.Url("trellis://" + string.Format("/org/{0}/member/{1}", organizationId, orgMemberId)));
        }
    
        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.UpdateOrganizationMember.Response>> UpdateOrganizationMember(string organizationId, string secondaryOrgMemberId, CreateAR.Trellis.Messages.UpdateOrganizationMember.Request request)
        {
            return _http.Put<CreateAR.Trellis.Messages.UpdateOrganizationMember.Response>(
                _http.Urls.Url("trellis://" + string.Format("/org/{0}/member/{1}", organizationId, secondaryOrgMemberId)),
                request);
        }

        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.DeleteOrganizationMember.Response>> DeleteOrganizationMember(string organizationId, string secondaryOrgMemberId)
        {
            return _http.Delete<CreateAR.Trellis.Messages.DeleteOrganizationMember.Response>(
                _http.Urls.Url("trellis://" + string.Format("/org/{0}/member/{1}", organizationId, secondaryOrgMemberId)));
        }
    }
}