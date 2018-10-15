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
            return _http.Post<CreateAR.Trellis.Messages.CreateFile.Response>(
                _http.Urls.Url("trellis://" + "/file"),
                request);
        }

        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.GetFile.Response>> GetFile(string fileId)
        {
            return _http.Get<CreateAR.Trellis.Messages.GetFile.Response>(
                _http.Urls.Url("trellis://" + string.Format("/file/{0}", fileId)));
        }
    
        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.GetMyFiles.Response>> GetMyFiles()
        {
            return _http.Get<CreateAR.Trellis.Messages.GetMyFiles.Response>(
                _http.Urls.Url("trellis://" + "/file"));
        }
    
        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.GetMyFilesByTags.Response>> GetMyFilesByTags(string fileTags)
        {
            return _http.Get<CreateAR.Trellis.Messages.GetMyFilesByTags.Response>(
                _http.Urls.Url("trellis://" + string.Format("/file?tags={0}", fileTags)));
        }
    
        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.UpdateFileInfo.Response>> UpdateFileInfo(string fileId, CreateAR.Trellis.Messages.UpdateFileInfo.Request request)
        {
            return _http.Put<CreateAR.Trellis.Messages.UpdateFileInfo.Response>(
                _http.Urls.Url("trellis://" + string.Format("/file/{0}", fileId)),
                request);
        }

        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.UpdateFile.Response>> UpdateFile(string fileId, CreateAR.Trellis.Messages.UpdateFile.Request request)
        {
            return _http.Put<CreateAR.Trellis.Messages.UpdateFile.Response>(
                _http.Urls.Url("trellis://" + string.Format("/file/{0}", fileId)),
                request);
        }

        public IAsyncToken<HttpResponse<CreateAR.Trellis.Messages.DeleteFile.Response>> DeleteFile(string fileId)
        {
            return _http.Delete<CreateAR.Trellis.Messages.DeleteFile.Response>(
                _http.Urls.Url("trellis://" + string.Format("/file/{0}", fileId)));
        }
    }
}