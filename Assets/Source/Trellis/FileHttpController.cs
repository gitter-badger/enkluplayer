/**
 * NETIFY GENERATED CODE: DO NOT EDIT.
 */
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;

namespace CreateAR.Trellis.Messages
{
    public class FileHttpController
    {
    
        private readonly IHttpService _http;
        
        public FileHttpController(IHttpService http)
        {
            _http = http;
        }

        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.CreateFile.Response>> CreateFile(CreateAR.Trellis.Messages.CreateFile.Request request)
        {   
            // Headers: [ Authorization ]
            return _http.Post<CreateAR.Trellis.Messages.CreateFile.Response>(
                "trellis://" + "/file",
                request);
        }

        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.GetFile.Response>> GetFile(string fileId)
        {
            // Headers: [ Authorization, Content-Type ]
            return _http.Get<CreateAR.Trellis.Messages.GetFile.Response>(
                "trellis://" + string.Format("/file/{0}", fileId));
        }
    
        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.GetMyFiles.Response>> GetMyFiles()
        {
            // Headers: [ Authorization, Content-Type ]
            return _http.Get<CreateAR.Trellis.Messages.GetMyFiles.Response>(
                "trellis://" + "/file");
        }
    
        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.GetMyFilesByTags.Response>> GetMyFilesByTags(string fileTags)
        {
            // Headers: [ Authorization, Content-Type ]
            return _http.Get<CreateAR.Trellis.Messages.GetMyFilesByTags.Response>(
                "trellis://" + string.Format("/file?tags={0}", fileTags));
        }
    
        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.UpdateFileInfo.Response>> UpdateFileInfo(string fileId, CreateAR.Trellis.Messages.UpdateFileInfo.Request request)
        {   
            // Headers: [ Authorization ]
            return _http.Put<CreateAR.Trellis.Messages.UpdateFileInfo.Response>(
                "trellis://" + string.Format("/file/{0}", fileId),
                request);
        }

        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.UpdateFile.Response>> UpdateFile(string fileId, CreateAR.Trellis.Messages.UpdateFile.Request request)
        {   
            // Headers: [ Authorization ]
            return _http.Put<CreateAR.Trellis.Messages.UpdateFile.Response>(
                "trellis://" + string.Format("/file/{0}", fileId),
                request);
        }

        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.DeleteFile.Response>> DeleteFile(string fileId)
        {
            // Headers: [ Authorization ]
            return _http.Delete<CreateAR.Trellis.Messages.DeleteFile.Response>(
                "trellis://" + string.Format("/file/{0}", fileId));
        }
        }
}