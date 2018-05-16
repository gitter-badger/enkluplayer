using System;
using System.Collections;
using System.IO;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using UnityEngine;

namespace CreateAR.SpirePlayer.Qr
{
    public class ScreenGrabber : MonoBehaviour
    {
        private AsyncToken<string> _grabToken;
        private Texture2D _texture;
        
        public IAsyncToken<string> Grab()
        {
            _grabToken = new AsyncToken<string>();
            
            StartCoroutine(StartGrab());

            return _grabToken;
        }

        private IEnumerator StartGrab()
        {
            var path = Guid.NewGuid() + ".png";
            
            ScreenCapture.CaptureScreenshot(path);

            var fullPath = Path.Combine(
                UnityEngine.Application.persistentDataPath,
                path);
            
            while (!File.Exists(fullPath))
            {
                yield return new WaitForSecondsRealtime(0.1f);   
            }
            
            _grabToken.Succeed(fullPath);
        }
    }
}