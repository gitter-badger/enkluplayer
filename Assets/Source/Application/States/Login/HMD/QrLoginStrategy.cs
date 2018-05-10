using System;
using System.Collections;
using System.Text;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.Qr;
using CreateAR.Trellis.Messages;
using CreateAR.Trellis.Messages.HoloSignin;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Logs a user in via Qr code.
    /// </summary>
    public class QrLoginStrategy : ILoginStrategy
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
        /// Service that reads QR codes from camera.
        /// </summary>
        private readonly IQrReaderService _qr;

        /// <summary>
        /// Makes API calls.
        /// </summary>
        private readonly ApiController _api;

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
        /// Internal token.
        /// </summary>
        private AsyncToken<CredentialsData> _loginToken;

        /// <summary>
        /// True iff login is executing.
        /// </summary>
        private bool _isAlive;

        /// <summary>
        /// Constructor.
        /// </summary>
        public QrLoginStrategy(
            IBootstrapper bootstrapper,
            IQrReaderService qr,
            ApiController api)
        {
            _bootstrapper = bootstrapper;
            _qr = qr;
            _api = api;
        }

        /// <inheritdoc />
        public IAsyncToken<CredentialsData> Login()
        {
            _loginToken = new AsyncToken<CredentialsData>();
            _loginToken.OnFinally(_ =>
            {
                _isAlive = false;

                // shutdown qr
                _qr.Stop();
                _qr.OnRead -= Qr_OnRead;

                UnityEngine.Object.Destroy(_root);

                // unload scene
                SceneManager.UnloadSceneAsync(
                    SceneManager.GetSceneByName(SCENE_NAME));
            });

            // load scene
            _bootstrapper.BootstrapCoroutine(WaitForScene(
                SceneManager.LoadSceneAsync(
                    SCENE_NAME,
                    LoadSceneMode.Additive)));

            _bootstrapper.BootstrapCoroutine(WatchToken());

            return _loginToken.Token();
        }

        /// <summary>
        /// Watches for timeouts.
        /// </summary>
        private IEnumerator WatchToken()
        {
            _isAlive = true;

            while (_isAlive)
            {
                if (null != _holoAuthToken)
                {
                    if (DateTime.Now.Subtract(_startRequest).TotalSeconds > TIMEOUT_SEC)
                    {
                        _view.ShowMessage("Request has timed out. Please double check your wifi connection.");

                        _holoAuthToken.Abort();
                        _holoAuthToken = null;
                    }
                }

                yield return null;
            }
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
            
            _startRequest = DateTime.Now;

            // make the call
            _holoAuthToken = _api
                .HoloAuths
                .HoloSignin(new Request
                {
                    Code = code
                });
            _holoAuthToken
                // always null for retries
                .OnFinally(_ => _holoAuthToken = null)
                .OnSuccess(response =>
                {
                    if (response.Payload.Success)
                    {
                        // fill out credentials
                        var creds = new CredentialsData
                        {
                            UserId = response.Payload.Body.UserId,
                            Token = response.Payload.Body.Token
                        };
                        
                        Log.Info(this, "HoloLogin complete.");
                        
                        _loginToken.Succeed(creds);
                    }
                    else
                    {
                        _view.ShowMessage("Could not login. Invalid QR code. Please contact support@enklu.com if this persists.");
                    }
                })
                .OnFailure(exception =>
                {
                    Log.Error(this, "Could not sign in with holocode : {0}", exception);
                    
                    _view.ShowMessage("Network error. Are you sure you're connected to the Internet? Please contact support@enklu.com if this persists.");
                });
        }
    }
}
