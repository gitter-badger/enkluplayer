/**
 * NETIFY GENERATED CODE: DO NOT EDIT.
 */
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;

namespace CreateAR.Trellis.Messages
{
    public class CollaboratorsHttpController
    {
    
        private readonly IHttpService _http;
        
        public CollaboratorsHttpController(IHttpService http)
        {
            _http = http;
        }

        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.GetAppCollaborators.Response>> GetAppCollaborators(string appId)
        {
            // Headers: [ Authorization ]
            return _http.Get<CreateAR.Trellis.Messages.GetAppCollaborators.Response>(
                "trellis://" + string.Format("/editor/app/{0}/collaborator", appId));
        }
    
        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.CreateAppCollaborator.Response>> CreateAppCollaborator(string appId, CreateAR.Trellis.Messages.CreateAppCollaborator.Request request)
        {   
            // Headers: [ Authorization ]
            return _http.Post<CreateAR.Trellis.Messages.CreateAppCollaborator.Response>(
                "trellis://" + string.Format("/editor/app/{0}/collaborator", appId),
                request);
        }

        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.UpdateAppCollaborator.Response>> UpdateAppCollaborator(string appId, string secondaryCollaboratorId, CreateAR.Trellis.Messages.UpdateAppCollaborator.Request request)
        {   
            // Headers: [ Authorization ]
            return _http.Put<CreateAR.Trellis.Messages.UpdateAppCollaborator.Response>(
                "trellis://" + string.Format("/editor/app/{0}/collaborator/{1}", appId, secondaryCollaboratorId),
                request);
        }

        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.DeleteAppCollaborator.Response>> DeleteAppCollaborator(string appId, string secondaryCollaboratorId)
        {
            // Headers: [ Authorization ]
            return _http.Delete<CreateAR.Trellis.Messages.DeleteAppCollaborator.Response>(
                "trellis://" + string.Format("/editor/app/{0}/collaborator/{1}", appId, secondaryCollaboratorId));
        }
        }
}