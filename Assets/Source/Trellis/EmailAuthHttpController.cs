/**
 * NETIFY GENERATED CODE: DO NOT EDIT.
 */
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;

namespace CreateAR.Trellis.Messages
{
    public class EmailAuthHttpController
    {
    
        private readonly IHttpService _http;
        
        public EmailAuthHttpController(IHttpService http)
        {
            _http = http;
        }

        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.EmailSignUp.Response>> EmailSignUp(CreateAR.Trellis.Messages.EmailSignUp.Request request)
        {
            return _http.Post<CreateAR.Trellis.Messages.EmailSignUp.Response>(
                _http.Urls.Url("trellis://" + "/email/signup"),
                request);
        }

        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.EmailSignIn.Response>> EmailSignIn(CreateAR.Trellis.Messages.EmailSignIn.Request request)
        {
            return _http.Post<CreateAR.Trellis.Messages.EmailSignIn.Response>(
                _http.Urls.Url("trellis://" + "/email/signin"),
                request);
        }

        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.EmailSignUpWithLicense.Response>> EmailSignUpWithLicense(CreateAR.Trellis.Messages.EmailSignUpWithLicense.Request request)
        {
            return _http.Post<CreateAR.Trellis.Messages.EmailSignUpWithLicense.Response>(
                _http.Urls.Url("trellis://" + "/email/signup"),
                request);
        }
}
}