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
using ZXing.Datamatrix.Encoder;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Logs a user in via Qr code.
    /// </summary>
    public class QrLoginApplicationState : IState
    {
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
        /// Makes API calls.
        /// </summary>
        private readonly ApiController _api;

        /// <summary>
        /// App-wide config.
        /// </summary>
        private readonly ApplicationConfig _config;

        /// <summary>
        /// Token returned from network.
        /// </summary>
        private IAsyncToken<HttpResponse<Response>> _holoAuthToken;

        /// <summary>
        /// Constructor.
        /// </summary>
        public QrLoginApplicationState(
            IBootstrapper bootstrapper,
            IMessageRouter messages,
            IQrReaderService qr,
            ApiController api,
            ApplicationConfig config)
        {
            _bootstrapper = bootstrapper;
            _messages = messages;
            _qr = qr;
            _api = api;
            _config = config;
        }

        /// <inheritdoc />
        public void Enter(object context)
        {
            // load scene
            _bootstrapper.BootstrapCoroutine(WaitForScene(
                UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(
                    SCENE_NAME,
                    LoadSceneMode.Additive)));
        }

        /// <inheritdoc />
        public void Update(float dt)
        {

        }

        /// <inheritdoc />
        public void Exit()
        {
            _qr.Stop();
            _qr.OnRead -= Qr_OnRead;

            var qr = GameObject.Find("Qr");
            qr.GetComponent<Image>().enabled = false;

            // unload scene
            UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(
                UnityEngine.SceneManagement.SceneManager.GetSceneByName(SCENE_NAME));
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

            var qr = GameObject.Find("Qr");
            qr.GetComponent<Image>().enabled = true;
            
            // start qr reader
            _qr.OnRead += Qr_OnRead;
            _qr.Start();
        }

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
                Log.Warning(this, "Invalid QR code value : {0}.", value);
                return;
            }

            var code = substrings[0];
            var appId = substrings[1];

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

                        // fill out app data
                        _config.Play.AppId = appId;

                        // load app
                        _messages.Publish(MessageTypes.LOAD_APP);
                    }
                    else
                    {
                        Log.Error(this, "Server refused our code : {0}.", response.Payload.Error);
                    }
                })
                .OnFailure(exception =>
                {
                    Log.Error(this, "Could not sign in with holocode : {0}.", exception);
                })
                .OnFailure(_ => _holoAuthToken = null);
        }
    }
}