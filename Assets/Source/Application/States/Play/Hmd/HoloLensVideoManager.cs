#if NETFX_CORE

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Trellis.Messages.CreateSnap;
using Newtonsoft.Json;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.EnkluPlayer
{
    // TODO: Throttle concurrent uploads
    // TODO: Retry failed uploads
    
    public class HoloLensVideoManager : IVideoManager
    {
        /// <summary>
        /// Dependencies
        /// </summary>
        private readonly IVideoCapture _videoCapture;
        private readonly IHttpService _http;
        private readonly UserPreferenceService _preferences;

        private string _orgId = string.Empty;

        private bool _enabled;
        
        private string _tag = "default";

        private AsyncToken<Void> _uploadToken;

        private List<string> _waitingUploads = new List<string>();
        private List<string> _failedUploads = new List<string>();
        
        public HoloLensVideoManager(IVideoCapture videoCapture, IHttpService http, UserPreferenceService preferences)
        {
            _videoCapture = videoCapture;
            _http = http;
            _preferences = preferences;

            _videoCapture.OnVideoCreated += Video_OnCreated;
        }

        public void EnableUploads(string tag)
        {
            _tag = !string.IsNullOrEmpty(tag) ? tag : "default";
            _enabled = true;
        }

        public void DisableUploads()
        {
            _enabled = false;
        }

        private void Video_OnCreated(string file)
        {
            Log.Info(this, "Video_OnCreated");

            _waitingUploads.Add(file);
            ProcessUploads();
        }

        private void ProcessUploads()
        {
            Log.Info(this, "ProcessUploads");

            // If we're uploading, early out. The upload will reprocess upon completion.
            if (_uploadToken != null)
            {
                return;
            }

            string currentUpload;
            if (_waitingUploads.Count > 0) // Process a new upload.
            {   
                currentUpload = _waitingUploads[0];
                _waitingUploads.RemoveAt(0);
            } 
            else if (_failedUploads.Count > 0) // Process a previous failure.
            {   
                currentUpload = _failedUploads[0];
                _failedUploads.RemoveAt(0);
            }
            else // Nothing waiting. All done here.
            {
                return;
            }
            
            _uploadToken = new AsyncToken<Void>();
            _uploadToken
                .OnSuccess(_ =>
                {
                    Log.Info(this, "Upload successful. Deleting from disk: ", currentUpload);
                    File.Delete(currentUpload);
                })
                .OnFailure(exeception =>
                {
                    Log.Error(this, "Upload failed.");
                    _failedUploads.Add(currentUpload);
                })
                .OnFinally(_ =>
                {
                    _uploadToken = null;
                    ProcessUploads();
                });
            
            GetOrgId()
                .OnSuccess(_ =>
                {
                    UploadFile(currentUpload, _uploadToken);
                })
                .OnFailure(exception =>
                {
                    Log.Error(this, "Error uploading video ({0})", exception);
                    _uploadToken.Fail(exception);
                });
        }

        private async Task UploadFile(string filepath, AsyncToken<Void> token)
        {
            Log.Info(this, "UploadFile");
            try 
            {
                var url = _http.Urls.Url(string.Format("/org/{0}/snap/gamma", _orgId));
                    
                Log.Info(this, "Uploading {0} to {1}", filepath, url);

                var httpClient = new HttpClient();
                foreach (var kvp in _http.Headers)
                {
                    httpClient.DefaultRequestHeaders.Add(kvp.Key, kvp.Value);
                }
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

                var form = new MultipartFormDataContent("Boundary7MA4YWxkTrZu0gW");
                var contentType = new MediaTypeHeaderValue("multipart/form-data");
                contentType.Parameters.Add(new NameValueHeaderValue("boundary", "Boundary7MA4YWxkTrZu0gW"));
                form.Headers.ContentType = contentType;

                var typeContent = new StringContent("video");
                typeContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                {
                    Name = "\"type\"",
                };
                form.Add(typeContent, "type");

                var tagContent = new StringContent(_tag);
                tagContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                {
                    Name = "\"tag\"",
                };
                form.Add(tagContent, "tag");

                var fileStream = new FileStream(filepath, FileMode.Open);
                var fileContent = new StreamContent(fileStream);
                fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                {
                    Name = "\"file\"",
                    FileName = "\"" + "testVideo.mp4" + "\""
                };
                fileContent.Headers.ContentType = new MediaTypeHeaderValue("video/mp4");
                form.Add(fileContent, "file");

                var rawResponse = await httpClient.PostAsync(url, form);
                var response = JsonConvert.DeserializeObject<Response>(await rawResponse.Content.ReadAsStringAsync());

                fileStream.Dispose();
                
                if (response.Success)
                {
                    token.Succeed(Void.Instance);
                }
                else
                {
                    token.Fail(new Exception(response.Error));
                }
            }
            catch (Exception e)
            {
                Log.Error(this, "Error uploading: {0}", e);
                token.Fail(e);
            }
        }

        private IAsyncToken<Void> GetOrgId()
        {
            var rtnToken = new AsyncToken<Void>();

            if (!string.IsNullOrEmpty(_orgId))
            {
                rtnToken.Succeed(Void.Instance);
            }
            else
            {
                _preferences
                    .ForCurrentUser()
                    .OnSuccess(prefs =>
                    {
                        var org = prefs.Data.DeviceRegistrations.FirstOrDefault();

                        if (org == null)
                        {
                            rtnToken.Fail(new Exception("No organizations."));
                        }
                        else
                        {
                            _orgId = org.OrgId;
                            rtnToken.Succeed(Void.Instance);
                        }
                    });
            }

            return rtnToken;
        }
    }
}

#endif