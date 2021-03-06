using System;
using System.Collections;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using CreateAR.EnkluPlayer.AR;
using UnityEngine;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.EnkluPlayer
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
            
            var exception = context as Exception;
            
            Log.Info(this, "MobileArSetupApplicationState({0})", exception);
            
            // if an exception has been passed in OR camera permissions have
            // been specifically denied, open the error view
            if (null != exception || CameraUtilsNativeInterface.HasDeniedCameraPermissions)
            {
                Log.Info(this, "Opening error view.");

                OpenCameraSettingsError();
            }
            // otherwise, open the prompt
            else
            {
                Log.Info(this, "Open prompt.");
                
                _ui
                    .Open<MobileArPromptViewController>(new UIReference
                        {
                            UIDataId = "Ar.Prompt"
                        })
                    .OnSuccess(el =>
                    {
                        el.OnCancelArSetup += Prompt_OnCancelArSetup;
                        el.OnStartArService += Prompt_OnStartArService;
                    })
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
            _ui.Open<IUIElement>(new UIReference
            {
                UIDataId = UIDataIds.LOADING
            });
            
            _bootstrapper.BootstrapCoroutine(DelayArSetup());
        }

        /// <summary>
        /// Slight delay for starting the camera.
        /// </summary>
        /// <returns></returns>
        private IEnumerator DelayArSetup()
        {
            yield return new WaitForSecondsRealtime(0.5f);
            
            _ar.Setup(_arConfig);
            
            yield return new WaitForSeconds(0.5f);

            // remove load screen
            _ui.Pop();
            _ui
                .Open<MobileArScanningViewController>(new UIReference
                {
                    UIDataId = "Ar.Scanning"
                }, out _scanningId);

            TryFindFloor();
        }

        /// <summary>
        /// Tries to find the floor. May call itself on retries.
        /// </summary>
        private void TryFindFloor()
        {
            FindFloor()
                .OnSuccess(_ =>
                {
                    _ui.Close(_scanningId);
                    
                    _messages.Publish(MessageTypes.FLOOR_FOUND);
                })
                .OnFailure(exception =>
                {
                    Log.Error(this, "Could not find floor : {0}", exception);
                    
                    _ui.Close(_scanningId);

                    _ui
                        .Open<MobileErrorUIView>(new UIReference
                        {
                            UIDataId = UIDataIds.ERROR
                        })
                        .OnSuccess(el =>
                        {
                            el.Message =
                                "Enklu could not find a suitable surface. For best results, point the camera at a surface and move slowly from side to side.";
                            el.Action = "Try again";
                            el.OnOk += () =>
                            {
                                _ui.Pop();

                                TryFindFloor();
                            };
                        });
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
                    for (int i = 0, len = anchors.Count; i < len; i++)
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
                    if (deltaSec > _arConfig.MaxSearchSec)
                    {
                        token.Fail(new Exception("Timeout."));
                        yield break;
                    }
                }
                
                yield return null;
            }
        }
        
        /// <summary>
        /// Opens the UI for a camera settings error.
        /// </summary>
        private void OpenCameraSettingsError()
        {
            int errorId;
            _ui
                .Open<MobileErrorUIView>(new UIReference
                    {
                        UIDataId = UIDataIds.ERROR
                    },
                    out errorId)
                .OnSuccess(el =>
                {
                    el.Message = "Camera access is required to continue.";
                    el.Action = "Open Settings";
                    el.OnOk += CameraUtilsNativeInterface.OpenSettings;
                })
                .OnFailure(HandleCriticalFailure);
        }

        /// <summary>
        /// Called when the prompt view requests canceling.
        /// </summary>
        private void Prompt_OnCancelArSetup()
        {
            _messages.Publish(MessageTypes.USER_PROFILE);
        }
        
        /// <summary>
        /// Called when the prompt view requests ar service.
        /// </summary>
        private void Prompt_OnStartArService()
        {
            _ui.Pop();

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
                            
                            OpenCameraSettingsError();
                        }
                    });
            }
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
