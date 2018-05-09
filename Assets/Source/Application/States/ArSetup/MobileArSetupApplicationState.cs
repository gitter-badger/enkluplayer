using System;
using System.Collections;
using System.Runtime.InteropServices;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using CreateAR.SpirePlayer.AR;
using UnityEngine;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// State that guides a user through AR setup.
    /// </summary>
    public class MobileArSetupApplicationState : IState
    {
        /// <summary>
        /// Dependencies.
        /// </summary>
        private readonly IMessageRouter _messages;
        private readonly IBootstrapper _bootstrapper;
        private readonly IArService _ar;
        private readonly IUIManager _ui;
        private readonly ArServiceConfiguration _arConfig;
        
        /// <summary>
        /// Time at which we started looking for the floor.
        /// </summary>
        private DateTime _startFloorSearch;

        /// <summary>
        /// UI frame.
        /// </summary>
        private UIManagerFrame _frame;
        
        /// <summary>
        /// Scanning view ID.
        /// </summary>
        private int _scanningId;

        /// <summary>
        /// Constructor.
        /// </summary>
        public MobileArSetupApplicationState(
            IMessageRouter messages,
            IBootstrapper bootstrapper,
            IArService ar,
            IUIManager ui,
            ArServiceConfiguration arConfig)
        {
            _messages = messages;
            _bootstrapper = bootstrapper;
            _ar = ar;
            _ui = ui;
            _arConfig = arConfig;
        }
        
        /// <inheritdoc />
        public void Enter(object context)
        {
            _frame = _ui.CreateFrame();
            
            // retrieve views
            /*
            var root = GameObject.Find("ArSetup");
             
            _prompt = root.GetComponentInChildren<ArPromptViewController>(true);
            _prompt.OnStartArService += Prompt_OnStartArService;
            
            _scanning = root.GetComponentInChildren<ArScanningViewController>(true);
            
            _error = root.GetComponentInChildren<ArErrorViewController>(true);
            _error.OnEnableCamera += Error_OnEnableCamera;

            _interrupted = root.GetComponentInChildren<ArInterruptedViewController>(true);
            */
            
            var exception = context as Exception;
            
            // if an exception has been passed in OR camera permissions have
            // been specifically denied, open the error view
            if (null != exception || CameraUtilsNativeInterface.HasDeniedCameraPermissions)
            {
                Log.Info(this, "Exception.");
                int errorId;
                _ui
                    .Open<ArErrorViewController>(new UIReference
                        {
                            UIDataId = UIDataIds.ERROR
                        },
                        out errorId)
                    .OnSuccess(el =>
                    {
                        el.OnOk += () =>
                        {
                            _ui.Close(errorId);
                            
                            OpenCameraSettings();
                        };
                    })
                    .OnFailure(HandleCriticalFailure);
            }
            // otherwise, open the prompt
            else
            {
                Log.Info(this, "Prompt.");
                _ui
                    .Open<ArPromptViewController>(new UIReference
                        {
                            UIDataId = "Ar.Prompt"
                        })
                    .OnSuccess(el => el.OnStartArService += Prompt_OnStartArService)
                    .OnFailure(HandleCriticalFailure);
            }
        }

        /// <inheritdoc />
        public void Update(float dt)
        {
            
        }

        /// <inheritdoc />
        public void Exit()
        {
            _frame.Release();
        }

        /// <summary>
        /// Starts the Ar service.
        /// </summary>
        private void StartArService()
        {
            _ar.Setup(_arConfig);

            FindFloor()
                .OnSuccess(_ =>
                {
                    _ui.Close(_scanningId);
                    
                    // watch tracking
                    _ar.OnTrackingOffline += Ar_OnTrackingOffline;
                    _ar.OnTrackingOnline += Ar_OnTrackingOnline;
                    
                    _messages.Publish(MessageTypes.FLOOR_FOUND);
                })
                .OnFailure(exception =>
                {
                    Log.Error(this, "Could not find floor : {0}", exception);
                    
                    // TODO: error prompt
                    _ui.Close(_scanningId);
                });
        }

        /// <summary>
        /// Finds the floor, tags it, then resolves the token.
        /// 
        /// TODO: Should be part of IArService?
        /// </summary>
        /// <returns></returns>
        private IAsyncToken<Void> FindFloor()
        {
            var token = new AsyncToken<Void>();

            Log.Info(this, "Attempting to find the floor.");
            
            _startFloorSearch = DateTime.Now;
            _bootstrapper.BootstrapCoroutine(PollAnchors(token));

            return token;
        }

        /// <summary>
        /// Polls the list of anchors for a floor. 
        /// </summary>
        /// <param name="token">The token to resolve when found.</param>
        /// <returns></returns>
        private IEnumerator PollAnchors(AsyncToken<Void> token)
        {
            while (true)
            {
                var deltaSec = (DateTime.Now.Subtract(_startFloorSearch).TotalSeconds);
                
                // wait at least min
                if (deltaSec < Mathf.Max(1, _arConfig.MinSearchSec))
                {
                    //
                }
                else
                {
                    // look for lowest anchor to call floor
                    var anchors = _ar.Anchors;
                    ArAnchor lowest = null;
                    for (int i = 0, len = anchors.Length; i < len; i++)
                    {
                        var anchor = anchors[i];
                        if (null == lowest
                            || anchor.Position.y < lowest.Position.y)
                        {
                            lowest = anchor;
                        }
                    }
                
                    // floor found!
                    if (null != lowest)
                    {
                        Log.Info(this, "Floor found : {0}.", lowest);
                        
                        // tag it
                        lowest.Tag(ArAnchorTags.FLOOR);
                        
                        // set camera rig
                        _arConfig.Rig.SetFloor(lowest);
                    
                        token.Succeed(Void.Instance);
                        yield break;
                    }
                
                    // waited too long!
                    // TODO: change the dialog.
                    /*if (deltaSec > _arConfig.MaxSearchSec)
                    {
                        token.Fail(new Exception("Timeout."));
                        yield break;
                    }*/
                }
                
                yield return null;
            }
        }
        
        /// <summary>
        /// Called when we've lost AR tracking.
        /// </summary>
        private void Ar_OnTrackingOffline()
        {
            Log.Info(this, "Ar tracking lost!");
            
            //_interrupted.gameObject.SetActive(true);
        }
        
        /// <summary>
        /// Called when AR tracking is back online.
        /// </summary>
        private void Ar_OnTrackingOnline()
        {
            Log.Info(this, "Ar tracking back online.");
            
            //_interrupted.gameObject.SetActive(false);
        }
        
        /// <summary>
        /// Called when the prompt view requests ar service.
        /// </summary>
        private void Prompt_OnStartArService()
        {
            _ui.Pop();

            _ui
                .Open<ArScanningViewController>(new UIReference
                {
                    UIDataId = "Ar.Scanning"
                }, out _scanningId);

            // we have camera permission
            if (CameraUtilsNativeInterface.HasCameraPermissions)
            {
                Log.Info(this, "Application has camera access, proceed to starting AR service.");
                
                StartArService();
            }
            // we have never asked for camera permission
            else
            {
                Log.Info(this, "Requesting camera access.");
                
                CameraUtilsNativeInterface.RequestCameraAccess(
                    _bootstrapper,
                    granted =>
                    {
                        if (granted)
                        {
                            Log.Info(this, "Camera access has been granted.");
                            
                            StartArService();
                        }
                        else
                        {
                            Log.Info(this, "Camera access was denied. Show error.");
                            
                            // the user denied access
                            /*_scanning.gameObject.SetActive(false);
                            _error.gameObject.SetActive(true);*/
                        }
                    });
            }
        }
        
        /// <summary>
        /// Opens settings.
        /// </summary>
        private void OpenCameraSettings()
        {
            Log.Info(this, "Open settings.");
            
            CameraUtilsNativeInterface.OpenSettings();
        }
        
        /// <summary>
        /// Handles a critical failure.
        /// </summary>
        /// <param name="ex">The exception.</param>
        private void HandleCriticalFailure(Exception ex)
        {
            Log.Fatal(this, "Critical failure : {0}.", ex);

            _messages.Publish(MessageTypes.FATAL_ERROR, ex);
        }
    }
}