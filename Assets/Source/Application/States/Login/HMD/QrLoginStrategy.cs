using System;
using System.Collections;
using System.Text;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using CreateAR.EnkluPlayer.Qr;
using CreateAR.Trellis.Messages;
using CreateAR.Trellis.Messages.HoloSignin;

namespace CreateAR.EnkluPlayer
{
    /// <inheritdoc />
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
        /// Bootstraps coroutines.
        /// </summary>
        private readonly IBootstrapper _bootstrapper;

        /// <summary>
        /// Service that reads QR codes from camera.
        /// </summary>
        private readonly IQrReaderService _qr;

        /// <summary>
        /// UI.
        /// </summary>
        private readonly IUIManager _ui;

        /// <summary>
        /// Messages.
        /// </summary>
        private readonly IMessageRouter _messages;

        /// <summary>
        /// Makes API calls.
        /// </summary>
        private readonly ApiController _api;
        
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
        /// UI id for QR.
        /// </summary>
        private int _qrId;

        /// <summary>
        /// UI id for env select.
        /// </summary>
        private int _envSelectId;

        /// <summary>
        /// Constructor.
        /// </summary>
        public QrLoginStrategy(
            IBootstrapper bootstrapper,
            IQrReaderService qr,
            IUIManager ui,
            IMessageRouter messages,
            ApiController api)
        {
            _bootstrapper = bootstrapper;
            _qr = qr;
            _ui = ui;
            _messages = messages;
            _api = api;
        }

        /// <inheritdoc />
        public IAsyncToken<CredentialsData> Login()
        {
            _qrId = -1;

            _loginToken = new AsyncToken<CredentialsData>();
            _loginToken.OnFinally(_ =>
            {
                _isAlive = false;

                _ui.Close(_qrId);
                _ui.Close(_envSelectId);

                // shutdown qr
                _qr.Stop();
                _qr.OnRead -= Qr_OnRead;
            });

            OpenQrReader();
            
            _qr.OnRead += Qr_OnRead;
            _qr.Start();

            _bootstrapper.BootstrapCoroutine(WatchToken());

            return _loginToken.Token();
        }

        /// <summary>
        /// Opens Qr reader view.
        /// </summary>
        private void OpenQrReader()
        {
            _ui
                .Open<QrViewController>(new UIReference
                {
                    UIDataId = "Qr.Login"
                }, out _qrId)
                .OnSuccess(el =>
                {
                    _view = el;
                    _view.OnConfigure += () =>
                    {
                        _ui.Close(_qrId);

                        OpenEnvSelect();
                    };
                })
                .OnFailure(ex => Log.Error(this, "Could not open Qr.Scanning : {0}.", ex));
        }

        /// <summary>
        /// Opens environment selection view.
        /// </summary>
        private void OpenEnvSelect()
        {
            _ui
                .Open<EnvSelectController>(new UIReference
                {
                    UIDataId = "EnvSelection"
                }, out _envSelectId)
                .OnSuccess(envSelect => envSelect.OnEnvironmentSelected += EnvSelect_OnSelected)
                .OnFailure(exception => Log.Error(this, "Could not open EnvSelectController: {0}.", exception));
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
        /// Called when environment has been selected.
        /// </summary>
        /// <param name="env">The environment data.</param>
        private void EnvSelect_OnSelected(EnvironmentData env)
        {
            _ui.Close(_envSelectId);

            _messages.Publish(MessageTypes.ENV_INFO_UPDATE, env);

            OpenQrReader();
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
                    Log.Error(this, "Could not sign in with holo-code : {0}", exception);
                    
                    _view.ShowMessage("Network error. Are you sure you're connected to the Internet? Please contact support@enklu.com if this persists.");
                });
        }
    }
}
