#if NETFX_CORE

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using UnityEngine;
using Void = CreateAR.Commons.Unity.Async.Void;
using Windows.Networking.BackgroundTransfer;

namespace CreateAR.EnkluPlayer
{
    // TODO: Throttle concurrent uploads
    // TODO: Retry failed uploads
    
    public class HoloLensVideoManager
    {
        /// <summary>
        /// Dependencies
        /// </summary>
        private readonly IVideoCapture _videoCapture;
        private readonly IHttpService _http;
        private readonly IBootstrapper _bootstrapper;
        private readonly UserPreferenceService _preferences;

        private string _orgId = string.Empty;
        
        public HoloLensVideoManager(IVideoCapture videoCapture, IHttpService http, IBootstrapper bootstrapper, UserPreferenceService preferences)
        {
            _videoCapture = videoCapture;
            _http = http;
            _bootstrapper = bootstrapper;
            _preferences = preferences;

            _videoCapture.OnVideoCreated += Video_OnCreated;
        }

        private void Video_OnCreated(string file)
        {
            GetOrgId()
                .OnSuccess(_ =>
                {
                    UploadFile(file);
                })
                .OnFailure(exception =>
                {
                    Log.Error(this, "Error uploading video ({0})", exception);
                });
        }

        private async Task UploadFile(string filepath)
        {
            try 
            {
                var url = _http.Urls.Url(string.Format("/org/{0}/snap/gamma", _orgId));
                    
                Log.Info(this, "Uploading {0} to {1}", filepath, url);

// System.Net
                var httpClient = new HttpClient();
                foreach (var kvp in _http.Headers)
                {
                    Log.Info(this, "{0} : {1}", kvp.Key, kvp.Value);
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

                var tagContent = new StringContent("holotest");
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

                var response = await httpClient.PostAsync(url, form);
                Log.Info(this, "Status Code: {0}", response.StatusCode);
                Log.Info(this, "Body: {0}", await response.Content.ReadAsStringAsync());

//                fileStream.Dispose();
                
// Windows APIs
//                var fileStream = File.OpenRead(filepath);
//                var uploader = new BackgroundUploader();
//    
//                foreach (var kvp in _http.Headers)
//                {
//                    uploader.SetRequestHeader(kvp.Key, kvp.Value);
//                }
//
//                uploader.SetRequestHeader()
//                
//                var uploadOp = await uploader.CreateUploadFromStreamAsync(new Uri(url), fileStream.AsInputStream());
//                Log.Info(this, "Uploading!!");
//    
//                await uploadOp.StartAsync();
//
//                var responseInfo = uploadOp.GetResponseInformation();
//                Log.Info(this, "Status Code: " + responseInfo.StatusCode);
//                Log.Info(this, "Uri: " + responseInfo.ActualUri);
            }
            catch (Exception e)
            {
                Log.Error(this, "Error uploading: {0}", e);
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