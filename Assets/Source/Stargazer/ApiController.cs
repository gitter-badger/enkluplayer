/**
 * NETIFY GENERATED CODE: DO NOT EDIT.
 */
using CreateAR.Commons.Unity.Http;

namespace CreateAR.Stargazer.Messages
{
    public class ApiController
    {
        public readonly AuthMobileHoloLensHttpController AuthMobileHoloLens;
        public readonly SessionsHttpController Sessions;
        
        public ApiController(IHttpService http)
        {
            AuthMobileHoloLens = new AuthMobileHoloLensHttpController(http);
            Sessions = new SessionsHttpController(http);
        }
    }
}