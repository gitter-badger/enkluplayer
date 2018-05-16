using System;
using System.Collections;
using System.IO;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using UnityEngine;

namespace CreateAR.SpirePlayer.Qr
{
    /// <summary>
    /// Takes a screenshot asynchronously.
    /// </summary>
    public class ScreenGrabber : MonoBehaviour
    {
        /// <summary>
        /// Token for screen grab.
        /// </summary>
        private AsyncToken<string> _grabToken;
        
        /// <summary>
        /// Texture.
        /// </summary>
        private Texture2D _texture;
        
        /// <summary>
        /// Grabs the screen.
        /// </summary>
        /// <returns></returns>
        public IAsyncToken<string> Grab()
        {
            _grabToken = new AsyncToken<string>();
            
            StartCoroutine(StartGrab());

            return _grabToken;
        }

        /// <summary>
        /// Coroutine that checks for existence of screenshot before resolving.
        /// </summary>
        /// <returns></returns>
        private IEnumerator StartGrab()
        {
            var path = Guid.NewGuid() + ".png";
            
            ScreenCapture.CaptureScreenshot(path);

            var fullPath = UnityEngine.Application.isMobilePlatform
                ? Path.Combine(
                    UnityEngine.Application.persistentDataPath,
                    path)
                : path;
            
            while (!File.Exists(fullPath))
            {
                yield return new WaitForSecondsRealtime(0.1f);   
            }
            
            _grabToken.Succeed(fullPath);
        }
    }
}