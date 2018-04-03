﻿using System;
using System.Collections;
using System.Text;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using CreateAR.Trellis.Messages;
using CreateAR.Trellis.Messages.HoloSignin;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Logs a user in via Qr code.
    /// </summary>
    public class QrLoginApplicationState : IState
    {
        /// <summary>
        /// How long to wait for timeout.
        /// </summary>
        private const int TIMEOUT_SEC = 3;

        /// <summary>
        /// Name of the playmode scene to load.
        /// </summary>
        private const string SCENE_NAME = "Qr";
    
        /// <summary>
        /// Bootstraps coroutines.
        /// </summary>
        private readonly IBootstrapper _bootstrapper;

        /// <summary>
        /// Messages.
        /// </summary>
        private readonly IMessageRouter _messages;

        /// <summary>
        /// Service that reads QR codes from camera.
        /// </summary>
        private readonly IQrReaderService _qr;

        /// <summary>
        /// Http service.
        /// </summary>
        private readonly IHttpService _http;
        
        /// <summary>
        /// Makes API calls.
        /// </summary>
        private readonly ApiController _api;

        /// <summary>
        /// App-wide config.
        /// </summary>
        private readonly ApplicationConfig _config;

        /// <summary>
        /// Root of UI.
        /// </summary>
        private GameObject _root;

        /// <summary>
        /// View controller.
        /// </summary>
        private QrViewController _view;
        
        /// <summary>
        /// Token returned from network.
        /// </summary>
        private IAsyncToken<HttpResponse<Response>> _holoAuthToken;

        /// <summary>
        /// Time at which request was sent.
        /// </summary>
        private DateTime _startRequest;

        /// <summary>
        /// Constructor.
        /// </summary>
        public QrLoginApplicationState(
            IBootstrapper bootstrapper,
            IMessageRouter messages,
            IQrReaderService qr,
            IHttpService http,
            ApiController api,
            ApplicationConfig config)
        {
            _bootstrapper = bootstrapper;
            _messages = messages;
            _qr = qr;
            _http = http;
            _api = api;
            _config = config;
        }

        /// <inheritdoc />
        public void Enter(object context)
        {
            // load scene
            _bootstrapper.BootstrapCoroutine(WaitForScene(
                SceneManager.LoadSceneAsync(
                    SCENE_NAME,
                    LoadSceneMode.Additive)));
        }

        /// <inheritdoc />
        public void Update(float dt)
        {
            if (null != _holoAuthToken)
            {
                if (DateTime.Now.Subtract(_startRequest).TotalSeconds > TIMEOUT_SEC)
                {
                    _view.ShowMessage("Request has timed out. Please double check your wifi connection.");
                }
            }
        }

        /// <inheritdoc />
        public void Exit()
        {
            // shutdown qr
            _qr.Stop();
            _qr.OnRead -= Qr_OnRead;

            UnityEngine.Object.Destroy(_root);

            // unload scene
            SceneManager.UnloadSceneAsync(
                SceneManager.GetSceneByName(SCENE_NAME));
        }

        /// <summary>
        /// Waits for scene to load.
        /// </summary>
        /// <param name="op">The scene load operation.</param>
        /// <returns></returns>
        private IEnumerator WaitForScene(AsyncOperation op)
        {
            yield return op;

            Log.Info(this, "Loaded Qr scene.");
            
            _root = new GameObject("Qr");
            _view = _root.AddComponent<QrViewController>();

            // start qr
            _qr.OnRead += Qr_OnRead;
            _qr.Start();
        }
        
        /// <summary>
        /// Called when the QR service reads a value.
        /// </summary>
        /// <param name="value">The value!</param>
        private void Qr_OnRead(string value)
        {
            if (null != _holoAuthToken)
            {
                return;
            }

            var bytes = Convert.FromBase64String(value);
            var decoded = Encoding.UTF8.GetString(bytes);
            var substrings = decoded.Split(':');
            if (2 != substrings.Length)
            {
                _view.ShowMessage("Invalid QR code. Look at HoloLogin code at https://editor.enklu.com.");
                Log.Warning(this, "Invalid QR code value : {0}.", value);
                return;
            }

            var code = substrings[0];
            var appId = substrings[1];

            _startRequest = DateTime.Now;

            // make the call
            _holoAuthToken = _api
                .HoloAuths
                .HoloSignin(new Request
                {
                    Code = code
                });
            _holoAuthToken.OnSuccess(response =>
                {
                    if (response.Payload.Success)
                    {
                        // fill out credentials
                        var creds = _config.Network.Credentials(_config.Network.Current);
                        creds.UserId = response.Payload.Body.UserId;
                        creds.Token = response.Payload.Body.Token;

                        creds.Apply(_http);

                        // fill out app data
                        _config.Play.AppId = appId;

                        Log.Info(this, "HoloLogin complete.");
                        
                        _messages.Publish(MessageTypes.LOGIN_COMPLETE);
                    }
                    else
                    {
                        Log.Error(this, "Server refused our code : {0}.", response.Payload.Error);

                        _view.ShowMessage("Could not login. Invalid account. Please contact support@enklu.com if this persists.");
                    }
                })
                .OnFailure(exception =>
                {
                    Log.Error(this, "Could not sign in with holocode : {0}.", exception);

                    _holoAuthToken = null;

                    _view.ShowMessage("Could not login. Please try again later. Please contact support@enklu.com if this persists.");
                });
        }
    }
}
