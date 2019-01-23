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
    /// <summary>
    /// VideoManager for HoloLens. Handles uploading videos to Trellis
    /// and managing failed uploads with a set cap.
    /// </summary>
    public class HoloLensVideoManager : IVideoManager
    {
        /// <summary>
        /// Dependencies
        /// </summary>
        private readonly IVideoCapture _videoCapture;
        private readonly IHttpService _http;
        private readonly UserPreferenceService _preferences;

        /// <summary>
        /// Cached user org.
        /// </summary>
        private string _orgId = string.Empty;

        /// <summary>
        /// Whether videos should be uploaded or not.
        /// </summary>
        private bool _enabled;
        
        /// <summary>
        /// The tag used to upload videos.
        /// </summary>
        private string _tag;

        /// <summary>
        /// Whether the last upload was successful or not.
        /// </summary>
        private bool _previouslyFailed = false;

        /// <summary>
        /// Current upload's token;
        /// </summary>
        private AsyncToken<Void> _uploadToken;

        /// <summary>
        /// Filepaths created this session, uploaded with priority over failures.
        /// </summary>
        private List<string> _waitingUploads = new List<string>();
        
        /// <summary>
        /// Filepaths that have failed in a prior session or this session.
        /// </summary>
        private List<string> _failedUploads = new List<string>();

        /// <summary>
        /// Configuration for Snaps.
        /// </summary>
        private SnapConfig _snapConfig;
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public HoloLensVideoManager(
            IVideoCapture videoCapture, 
            IHttpService http, 
            UserPreferenceService preferences,
            ApplicationConfig applicationConfig)
        {
            _videoCapture = videoCapture;
            _http = http;
            _preferences = preferences;
            _snapConfig = applicationConfig.Snap;

            _videoCapture.OnVideoCreated += Video_OnCreated;
        }

        /// <summary>
        /// Starts the upload process for pending videos & newly created ones.
        /// </summary>
        /// <param name="tag">The tag to send to Trellis.</param>
        /// <param name="uploadExisting">Whether files on disk should be retried or not.</param>
        public void EnableUploads(string tag, bool uploadExisting = false)
        {
            _tag = !string.IsNullOrEmpty(tag) ? tag : "default";
            
            if (uploadExisting)
            {
                // Find saves already on the device. If they're there, it means they failed previously.
                var root = Path.Combine(UnityEngine.Application.persistentDataPath, _snapConfig.VideoFolder);
                if (Directory.Exists(root))
                {
                    FindExistingUploads(root, _failedUploads);

                    if (_failedUploads.Count > 0)
                    {
                        Log.Info(this, "Previously failed uploads: {0}", _failedUploads.Count);
                    }
                }
            }
            
            _enabled = true;
            ProcessUploads();
        }

        /// <summary>
        /// Disables uploads. If an upload is in progress, it will finish.
        /// </summary>
        public void DisableUploads()
        {
            _enabled = false;
        }

        /// <summary>
        /// Adds a video to the upload queue and attempts to start the upload process.
        /// </summary>
        /// <param name="file"></param>
        private void Video_OnCreated(string file)
        {
            _waitingUploads.Add(file);
            ProcessUploads();
        }

        /// <summary>
        /// Begins uploading an awaiting file if enabled.
        /// </summary>
        private void ProcessUploads()
        {
            // If we're uploading, or canceled early out. The upload will reprocess upon completion.
            if (_uploadToken != null || !_enabled)
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
                Log.Info(this, "All uploads finished.");
                return;
            }
            
            _uploadToken = new AsyncToken<Void>();
            _uploadToken
                .OnSuccess(_ =>
                {
                    _previouslyFailed = false;
                    Log.Info(this, "Upload successful. Deleting from disk: {0}", currentUpload);
                    File.Delete(currentUpload);
                })
                .OnFailure(exeception =>
                {
                    _previouslyFailed = true;
                    Log.Error(this, "Upload failed. ({0})", exeception);
                    _failedUploads.Add(currentUpload);
                    Upload_OnFailure();
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

        /// <summary>
        /// Uploads a file to the snaps endpoint. Resolves the token on completion.
        /// </summary>
        /// <param name="filepath">The full filepath.</param>
        /// <param name="token">The token to resolve.</param>
        /// <returns></returns>
        private async Task UploadFile(string filepath, AsyncToken<Void> token)
        {
            if (_previouslyFailed)
            {
                Log.Info(this, "Last upload failed. Waiting for 30 seconds.");
                await Task.Delay(_snapConfig.FailureDelayMilliseconds);
            }
            
            try 
            {
                var filename = filepath.Substring(filepath.LastIndexOf("\\") + 1);
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
                    FileName = "\"" + filename + "\""
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

        /// <summary>
        /// On upload failure, determine if videos should be removed from disk or kept 
        /// </summary>
        private void Upload_OnFailure()
        {
            if (_failedUploads.Count <= _snapConfig.MaxVideoUploads)
            {
                return;
            }
            
            Log.Info(this, "Too many failed recordings. Current: {0}  Limit: {1}", _failedUploads.Count, _snapConfig.MaxVideoUploads);
            
            // Not the most efficient since the creation times aren't cached,
            // but this shouldn't be run often or over a large dataset.
            _failedUploads.Sort((file1, file2) =>
                (int) (File.GetCreationTime(file2) - File.GetCreationTime(file1)).TotalSeconds);

            for (var i = _failedUploads.Count - 1; i >= _snapConfig.MaxVideoUploads; i--)
            {
                Log.Info(this, "Deleting failed upload: " + _failedUploads[i]);
                File.Delete(_failedUploads[i]);
                _failedUploads.RemoveAt(i);
            }
        }

        /// <summary>
        /// Recursively searches for files in the specified folder and adds them to the container.
        /// </summary>
        private void FindExistingUploads(string folder, List<string> container)
        {
            container.AddRange(Directory.GetFiles(folder));

            var subDirs = Directory.GetDirectories(folder);
            for (var i = 0; i < subDirs.Length; i++)
            {
                FindExistingUploads(subDirs[i], container);
            }
        }

        /// <summary>
        /// Returns the current user's organization ID, if available.
        /// </summary>
        /// <returns></returns>
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