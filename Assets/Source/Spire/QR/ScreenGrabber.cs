using System;
using System.Collections;
using System.IO;
using CreateAR.Commons.Unity.Async;
using UnityEngine;

namespace CreateAR.SpirePlayer.Qr
{
    public class ScreenGrabber : MonoBehaviour
    {
        private AsyncToken<string> _grabToken;
        private Texture2D _texture;
        
        public IAsyncToken<string> Grab()
        {
            return _grabToken = new AsyncToken<string>();
        }

        private IEnumerable StartGrab()
        {
            var path = Guid.NewGuid().ToString() + ".png";
            
            ScreenCapture.CaptureScreenshot(path);

            yield return new WaitForSecondsRealtime(0.25f);
            
            _grabToken.Succeed(Path.Combine(
                UnityEngine.Application.persistentDataPath,
                path));
        }
    }
}