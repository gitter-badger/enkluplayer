using System;
using System.Collections;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using CreateAR.SpirePlayer.Qr;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Designer for mobile.
    /// </summary>
    public class MobileDesignController : IDesignController
    {
        /// <summary>
        /// UI.
        /// </summary>
        private readonly IUIManager _ui;

        /// <summary>
        /// Messages/
        /// </summary>
        private readonly IMessageRouter _messages;

        /// <summary>
        /// Bootstraps coroutines.
        /// </summary>
        private readonly IBootstrapper _bootstrapper;
        
        /// <summary>
        /// Qr reader.
        /// </summary>
        private readonly IQrReaderService _qr;

        /// <summary>
        /// Menu.
        /// </summary>
        private MobilePlayModeMenu _menu;
        
        /// <summary>
        /// UI frame.
        /// </summary>
        private UIManagerFrame _frame;
        
        /// <summary>
        /// Lazily created render cam.
        /// </summary>
        private Camera _renderCamera;

        /// <summary>
        /// Constructor.
        /// </summary>
        public MobileDesignController(
            IUIManager ui,
            IMessageRouter messages,
            IBootstrapper bootstrapper,
            IQrReaderService qr)
        {
            _ui = ui;
            _messages = messages;
            _bootstrapper = bootstrapper;
            _qr = qr;
        }
        
        /// <inheritdoc />
        public void Setup(DesignerContext context, IAppController app)
        {
            _frame = _ui.CreateFrame();
            
            _ui
                .Open<MobilePlayModeMenu>(new UIReference
                {
                    UIDataId = "Play.Main"
                })
                .OnSuccess(el =>
                {
                    _menu = el;
                    
                    el.OnBackClicked += () => _messages.Publish(MessageTypes.USER_PROFILE);
                    el.OnInstaClicked += () =>
                    {
                        // get permission first
                        if (CameraUtilsNativeInterface.HasPhotosPermissions)
                        {
                            Insta();
                        }
                        else
                        {
                            CameraUtilsNativeInterface.RequestPhotosAccess(
                                _bootstrapper,
                                success =>
                                {
                                    if (success)
                                    {
                                        Insta();
                                    }
                                    else
                                    {
                                        _ui
                                            .Open<MobileErrorUIView>(new UIReference
                                            {
                                                UIDataId = UIDataIds.ERROR
                                            })
                                            .OnSuccess(err =>
                                            {
                                                err.Message = "Photo library access required for screenshot.";
                                                err.Action = "Okay";
                                                err.OnOk += () => _ui.Pop();
                                            })
                                            .OnFailure(exception => Log.Error(this, "Could not open error for library access : {0}", exception));   
                                    }
                                });
                        }
                    };
                })
                .OnFailure(ex => Log.Error(this, "Could not open Play.Main : {0}.", ex));

            _qr.Start();
        }

        /// <inheritdoc />
        public void Teardown()
        {
            _qr.Stop();
            
            _frame.Release();
            
            if (null != _renderCamera)
            {
                UnityEngine.Object.Destroy(_renderCamera.gameObject);
            }
        }

        /// <inheritdoc />
        public IAsyncToken<string> Create()
        {
            return new AsyncToken<string>(new NotImplementedException());
        }

        /// <inheritdoc />
        public void Select(string sceneId, string elementId)
        {
            //
        }
        
        /// <summary>
        /// Shares to Insta.
        /// </summary>
        private void Insta()
        {
            _bootstrapper.BootstrapCoroutine(TakePic(texture =>
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

            _menu.gameObject.SetActive(false);
            
            yield return new WaitForSeconds(0.1f);
            
            _renderCamera.transform.position = Camera.main.transform.position;
            _renderCamera.transform.rotation = Camera.main.transform.rotation;
            _renderCamera.transform.localScale = Camera.main.transform.localScale;
            
            var width = Screen.width;
            var height = Screen.height;
            var rt = RenderTexture.GetTemporary(
                width, height, 32,
                RenderTextureFormat.ARGB32,
                RenderTextureReadWrite.sRGB);

            RenderTexture.active = _renderCamera.targetTexture = rt;

            yield return new WaitForEndOfFrame();
            
            _renderCamera.Render();

            var texture = new Texture2D(width, height, TextureFormat.ARGB32, false);
            texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            texture.Apply();

            // cleanup
            RenderTexture.active = _renderCamera.targetTexture = null;
            RenderTexture.ReleaseTemporary(rt);

            _menu.gameObject.SetActive(true);
            
            callback(texture);
        }
    }
}