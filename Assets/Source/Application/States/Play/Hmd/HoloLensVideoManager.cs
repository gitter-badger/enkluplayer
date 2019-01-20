#if NETFX_CORE

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using UnityEngine;
using Void = CreateAR.Commons.Unity.Async.Void;
using Windows.Storage;

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
            Debug().Wait(20000);

            _videoCapture = videoCapture;
            _http = http;
            _bootstrapper = bootstrapper;
            _preferences = preferences;

            _videoCapture.OnVideoCreated += Video_OnCreated;
        }

        private async Task Debug() 
        {
            Log.Info(this, "Debug 3");
            try 
            {
                StorageFolder appInstalledFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                Log.Info(this, appInstalledFolder.Path);

                StorageFolder videoFolder = await appInstalledFolder.GetFolderAsync("videos");
                Log.Info(this, videoFolder);

                IReadOnlyList<StorageFile> fileList = await videoFolder.GetFilesAsync();
                foreach (StorageFile file in fileList)
                {
                    Log.Info(this, file);
                }

                StorageFile fuckingFile = await videoFolder.GetFileAsync("testVideo.mp4");
                Log.Info(this, fuckingFile);
            }
            catch (Exception e)
            {
                Log.Error(this, e);
            }

            Log.Info(this, "Debug 2");
            try 
            {
                var path = "C:/Data/Users/james/AppData/Local/Packages/Enklu_wyr6ev42crejm/LocalState/videos/testVideo.mp4";
                path = path.Replace("/", "\\");
                StorageFile file1 = await StorageFile.GetFileFromPathAsync(path);
                Log.Info(this, file1);
            }
            catch (Exception e)
            {
                Log.Error(this, e);
            }
        }

        private void Video_OnCreated(string file)
        {
            GetOrgId()
                .OnSuccess(_ =>
                {
                    UploadFile(file).Wait(10000);
                })
                .OnFailure(exception =>
                {
                    Log.Error(this, "Error uploading video ({0})", exception);
                });
        }

        private async Task UploadFile(string filepath)
        {
            var filename = filepath.Substring(filepath.LastIndexOf("/") + 1);
            var url = string.Format("/org/{0}/snap/gamma", _orgId);
                    
            Log.Info(this, "Uploading {0} to {1}", filepath, url);

            var uploader = new Windows.Networking.BackgroundTransfer.BackgroundUploader();

            foreach (var kvp in _http.Headers)
            {
                uploader.SetRequestHeader(kvp.Key, kvp.Value);
            }
            
            Log.Info(this, "Uploading!!");
//            await uploader.CreateUploadFromStreamAsync(new System.Uri(_http.Urls.Url(url)), new FileStream(filepath, FileMode.Open).AsInputStream()).StartAsync();

            Log.Info(this, "Uploaded?!");

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