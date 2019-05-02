/**
 * NETIFY GENERATED CODE: DO NOT EDIT.
 */
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;

namespace CreateAR.Trellis.Messages
{
    public class UserHttpController
    {
    
        private readonly IHttpService _http;
        
        public UserHttpController(IHttpService http)
        {
            _http = http;
        }

        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.GetUser.Response>> GetUser(string userid)
        {
            // Headers: [ Authorization, Content-Type ]
            return _http.Get<CreateAR.Trellis.Messages.GetUser.Response>(
                "trellis://" + string.Format("/user/{0}", userid));
        }
    
        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.SearchUsersById.Response>> SearchUsersById(string userid)
        {
            // Headers: [ Authorization, Content-Type ]
            return _http.Get<CreateAR.Trellis.Messages.SearchUsersById.Response>(
                "trellis://" + string.Format("/user?id={0}", userid));
        }
    
        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.SearchUsersByEmail.Response>> SearchUsersByEmail(string email)
        {
            // Headers: [ Authorization ]
            return _http.Get<CreateAR.Trellis.Messages.SearchUsersByEmail.Response>(
                "trellis://" + string.Format("/user?email={0}", email));
        }
        }
}