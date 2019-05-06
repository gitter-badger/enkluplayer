using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using CreateAR.EnkluPlayer.IUX;
using CreateAR.Trellis.Messages;
using CreateAR.Trellis.Messages.GetMyApps;
using UnityEngine;
using Object = System.Object;

namespace CreateAR.EnkluPlayer
{
    public class StandaloneMenuViewController : InjectableMonoBehaviour
    {
        private IAsyncToken<HttpResponse<Response>> _getAppsToken;

        private Exception _appsError;
        private Body[] _apps;
        private readonly List<CameraWidget> _cameras = new List<CameraWidget>();

        [Inject]
        public IMessageRouter Messages { get; set; }

        [Inject]
        public IAppSceneManager Scenes { get; set; }

        [Inject]
        public ApiController Api { get; set; }

        [Inject]
        public ApplicationConfig Config { get; set; }

        private void OnEnable()
        {
            // get apps
            _getAppsToken = Api
                .Apps
                .GetMyApps()
                .OnSuccess(res =>
                {
                    if (res.Payload.Success)
                    {
                        _appsError = null;
                        _apps = res.Payload.Body;
                    }
                    else
                    {
                        _apps = null;
                        _appsError = new Exception(res.Payload.Error);
                    }
                })
                .OnFailure(ex =>
                {
                    Log.Warning(this, "Could not get my apps: {0}.", ex);

                    _appsError = ex;
                });
            
            // get cameras
            var root = Scenes.Root(Scenes.All[0]);
            root.Find("..(@type==CameraWidget)", _cameras);
        }

        private void OnDisable()
        {
            _appsError = null;
            _apps = null;
            _cameras.Clear();

            if (null != _getAppsToken)
            {
                _getAppsToken.Abort();
                _getAppsToken = null;
            }
        }

        private void OnGUI()
        {
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            {
                GUILayout.FlexibleSpace();
                GUILayout.BeginVertical(GUILayout.Width(800));
                {
                    if (GUILayout.Button("Exit", GUILayout.Width(100)))
                    {
                        Destroy(this);
                    }

                    DrawCameraSelection();
                    DrawExperienceSelection();
                }
                GUILayout.EndVertical();
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();
        }
        
        private void DrawExperienceSelection()
        {
            GUILayout.BeginVertical("box");
            {
                if (null != _appsError)
                {
                    GUILayout.Label(string.Format(
                        "There was an error loading apps: {0}.",
                        _appsError.Message));
                }
                else if (null != _apps)
                {
                    foreach (var app in _apps)
                    {
                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.Label(app.Name, GUILayout.Width(400));

                            if (GUILayout.Button("Load", GUILayout.Width(100)))
                            {
                                Config.Play.AppId = app.Id;
                                Messages.Publish(MessageTypes.LOAD_APP);
                            }

                        }
                        GUILayout.EndHorizontal();
                    }
                }
                else
                {
                    GUILayout.Label("Loading user prefs.");
                }
            }
            GUILayout.EndVertical();
        }

        private void DrawCameraSelection()
        {
            GUILayout.BeginVertical("box");
            {
                foreach (var widget in _cameras)
                {
                    GUILayout.BeginHorizontal();
                    {
                        if (GUILayout.Button(widget.Name, GUILayout.Width(100)))
                        {
                            widget.Apply(Camera.main);
                        }
                    }
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndVertical();
        }
    }
}