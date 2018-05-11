﻿using System;
using System.Collections;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
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
            IBootstrapper bootstrapper)
        {
            _ui = ui;
            _messages = messages;
            _bootstrapper = bootstrapper;
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
                    
                    el.OnBackClicked += () =>
                    {
                        _messages.Publish(MessageTypes.USER_PROFILE);
                    };
                    
                    el.OnInstaClicked += () =>
                    {
                        _bootstrapper.BootstrapCoroutine(TakePic(texture =>
                        {
                            Log.Info(this, "Screenshot saved to {0}.", texture);

                            NativeGallery.SaveToGallery(texture, "ScreenShots", "ss.png");
                        }));
                    };
                })
                .OnFailure(ex => Log.Error(this, "Could not open Play.Main : {0}.", ex));
        }

        /// <inheritdoc />
        public void Teardown()
        {
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