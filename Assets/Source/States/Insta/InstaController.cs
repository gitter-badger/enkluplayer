using System;
using System.Collections;
using CreateAR.Commons.Unity.Logging;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Listens to insta UI controls.
    /// </summary>
    public class InstaController : MonoBehaviour
    {
        /// <summary>
        /// Lazily created render cam.
        /// </summary>
        private Camera _renderCamera;
        
        /// <summary>
        /// Called when the Insta button has been clicked.
        /// </summary>
        public void InstaBtn_OnClicked()
        {
            StartCoroutine(TakePic(texture =>
            {
                Log.Info(this, "Screenshot saved to {0}.", texture);
                
                NativeGallery.SaveToGallery(texture, "ScreenShots", "ss.png");
            }));
        }

        /// <summary>
        /// Takes a screenshot.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <returns></returns>
        private IEnumerator TakePic(Action<Texture2D> callback)
        {
            if (null == _renderCamera)
            {
                var root = new GameObject("ScreenshotCam");
                
                _renderCamera = root.AddComponent<Camera>();
                _renderCamera.enabled = false;
                _renderCamera.CopyFrom(Camera.main);
            }
            
            _renderCamera.transform.position = Camera.main.transform.position;
            _renderCamera.transform.rotation = Camera.main.transform.rotation;
            _renderCamera.transform.localScale = Camera.main.transform.localScale;
            
            var width = Screen.width;
            var height = Screen.height;
            var rt = RenderTexture.GetTemporary(
                width, height, 32,
                RenderTextureFormat.ARGB32,
                RenderTextureReadWrite.sRGB);
            
            // shut off UI
            //cam.cullingMask &=  ~(1 << LayerMask.NameToLayer("UI"));

            RenderTexture.active = _renderCamera.targetTexture = rt;

            yield return new WaitForEndOfFrame();
            
            // TODO: replace waiting a frame with new disabled camera
            /*yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();*/
            
            _renderCamera.Render();
            
            // restore UI layer
            //cam.cullingMask |= 1 << LayerMask.NameToLayer("UI");

            var texture = new Texture2D(width, height, TextureFormat.ARGB32, false);
            texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            texture.Apply();

            // cleanup
            RenderTexture.active = _renderCamera.targetTexture = null;
            RenderTexture.ReleaseTemporary(rt);

            callback(texture);
        }
        
        /// <inheritdoc />
        private void OnDestroy()
        {
            if (null != _renderCamera)
            {
                Destroy(_renderCamera.gameObject);
            }
        }
    }
}