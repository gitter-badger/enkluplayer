/**
 * NETIFY GENERATED CODE: DO NOT EDIT.
 */
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;

namespace CreateAR.Trellis.Messages
{
    public class DevicesHttpController
    {
    
        private readonly IHttpService _http;
        
        public DevicesHttpController(IHttpService http)
        {
            _http = http;
        }

        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.GetOrganizationDevices.Response>> GetOrganizationDevices(string organizationId)
        {
            return _http.Get<CreateAR.Trellis.Messages.GetOrganizationDevices.Response>(
                _http.Urls.Url("trellis://" + string.Format("/org/{0}/device", organizationId)));
        }
    
        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.GetOrganizationDevice.Response>> GetOrganizationDevice(string organizationId, string deviceId)
        {
            return _http.Get<CreateAR.Trellis.Messages.GetOrganizationDevice.Response>(
                _http.Urls.Url("trellis://" + string.Format("/org/{0}/device/{1}", organizationId, deviceId)));
        }
    
        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.CreateOrganizationDevice.Response>> CreateOrganizationDevice(string organizationId, CreateAR.Trellis.Messages.CreateOrganizationDevice.Request request)
        {
            return _http.Post<CreateAR.Trellis.Messages.CreateOrganizationDevice.Response>(
                _http.Urls.Url("trellis://" + string.Format("/org/{0}/device", organizationId)),
                request);
        }

        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.UpdateOrganizationDevice.Response>> UpdateOrganizationDevice(string organizationId, string deviceId, CreateAR.Trellis.Messages.UpdateOrganizationDevice.Request request)
        {
            return _http.Put<CreateAR.Trellis.Messages.UpdateOrganizationDevice.Response>(
                _http.Urls.Url("trellis://" + string.Format("/org/{0}/device/{1}", organizationId, deviceId)),
                request);
        }

        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.DeleteOrganizationDevice.Response>> DeleteOrganizationDevice(string organizationId, string deviceId)
        {
            return _http.Delete<CreateAR.Trellis.Messages.DeleteOrganizationDevice.Response>(
                _http.Urls.Url("trellis://" + string.Format("/org/{0}/device/{1}", organizationId, deviceId)));
        }
    }
}