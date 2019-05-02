/**
 * NETIFY GENERATED CODE: DO NOT EDIT.
 */
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;

namespace CreateAR.Stargazer.Messages
{
    public class AuthMobileHoloLensHttpController
    {
    
        private readonly IHttpService _http;
        
        public AuthMobileHoloLensHttpController(IHttpService http)
        {
            _http = http;
        }

        public IAsyncToken<HttpResponse<CreateAR.Stargazer.Messages.HololensMobileSignin.Response>> HololensMobileSignin(CreateAR.Stargazer.Messages.HololensMobileSignin.Request request)
        {   
            // Headers: [ Content-Type ]
            return _http.Post<CreateAR.Stargazer.Messages.HololensMobileSignin.Response>(
                "stargazer://" + "/auth/mobile/signin",
                request);
        }
    }
}