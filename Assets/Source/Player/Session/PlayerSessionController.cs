using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.DataStructures;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using CreateAR.EnkluPlayer.Qr;
using CreateAR.EnkluPlayer.Util;
using CreateAR.Stargazer.Messages;
using CreateAR.Stargazer.Messages.HololensMobileSignin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CreateAR.EnkluPlayer.Player.Session
{
    /// <summary>
    /// Controls all experience related interaction and stores current session data.
    /// </summary>
    public class PlayerSessionController : IPlayerSessionController
    {
        /// <summary>
        /// Bootstraps coroutines.
        /// </summary>
        //private readonly IBootstrapper _bootstrapper;

        /// <summary>
        /// Http Service used to make stargazer calls.
        /// </summary>
        private IHttpService _http;

        /// <summary>
        /// Service that reads QR codes from camera.
        /// </summary>
        private readonly IQrReaderService _qr;

        /// <summary>
        /// UI.
        /// </summary>
        private readonly IUIManager _ui;

        /// <summary>
        /// Makes API calls.
        /// </summary>
        private readonly ApiController _api;

        /// <summary>
        /// View controller.
        /// </summary>
        private SessionQrViewController _view;
        
        /// <summary>
        /// Player session token.
        /// </summary>
        private AsyncToken<PlayerSession> _sessionToken;

        /// <summary>
        /// Stargazer login token.
        /// </summary>
        private AsyncToken<StargazerCredentials> _loginToken;

        /// <summary>
        /// Token returned from network.
        /// </summary>
        private IAsyncToken<HttpResponse<Response>> _holoAuthToken;
        
        /// <summary>
        /// JSON serializer
        /// </summary>
        private JsonSerializer _json = new JsonSerializer();

        /// <summary>
        /// The current player session.
        /// </summary>
        private PlayerSession _currentSession;

        /// <summary>
        /// UI id for QR.
        /// </summary>
        private int _qrId;

        /// <summary>
        /// Dispatches whenever a new session is created.
        /// </summary>
        public event Action<PlayerSession> OnSessionCreated;

        /// <summary>
        /// Dispatches whenever a session ends.
        /// </summary>
        public event Action OnSessionEnded;

        /// <summary>
        /// The current player session.
        /// </summary>
        public PlayerSession CurrentSession
        {
            get
            {
                return _currentSession;
            }
        }

        /// <summary>
        /// Creates a new <see cref="PlayerSessionController"/> instance.
        /// </summary>
        public PlayerSessionController(
            //IBootstrapper bootstrapper,
            IHttpService http,
            IQrReaderService qr,
            IUIManager ui,
            ApiController api)
        {
            //_bootstrapper = bootstrapper;
            _http = http;
            _qr = qr;
            _ui = ui;
            _api = api;
        }

        /// <inheritdoc />
        public IAsyncToken<PlayerSession> CreateSession()
        {
            if (null != _sessionToken)
            {
                return _sessionToken.Token();
            }

            if (null != _currentSession)
            {
                return new AsyncToken<PlayerSession>(_currentSession);
            }

            _sessionToken = new AsyncToken<PlayerSession>();

            Login()
                .OnSuccess(creds =>
                {
                    // Apply Stargazer Credentials
                    creds.Apply(_http);

                    _api
                        .Sessions
                        .CreateSession(new Stargazer.Messages.CreateSession.Request())
                        .OnSuccess(response =>
                        {
                            if (response.Payload.Success)
                            {
                                // Create Player Session
                                _currentSession = new PlayerSession(creds.UserId, response.Payload.Body.SessionId);

                                _sessionToken.Succeed(_currentSession);

                                // Dispatch after executing callback
                                DispatchCreated();
                            }
                            else
                            {
                                var error = response.Payload.Error;
                                Log.Warning(this, "Failed to create session: {0}", error);

                                _sessionToken.Fail(new Exception(error));
                            }
                        })
                        .OnFailure(_sessionToken.Fail);
                })
                .OnFailure(_sessionToken.Fail);

            return _sessionToken.Token();
        }

        /// <inheritdoc/>
        public void EndSession()
        {
            var isDispatch = null != _currentSession;

            if (null != _sessionToken)
            {
                _sessionToken.Abort();
                _sessionToken = null;
            }

            if (null != _loginToken)
            {
                _loginToken.Abort();
                _loginToken = null;
            }

            if (null != _holoAuthToken)
            {
                _holoAuthToken.Abort();
                _holoAuthToken = null;
            }

            _currentSession = null;
            ClearHttpCredentials();

            if (isDispatch)
            {
                DispatchEnded();
            }
        }
        
        /// <summary>
        /// Logs into stargazer by reading a QR code.
        /// </summary>
        private IAsyncToken<StargazerCredentials> Login()
        {
            if (null != _loginToken)
            {
                return _loginToken.Token();
            }

            _qrId = -1;

            _loginToken = new AsyncToken<StargazerCredentials>();

            OpenQrReader();

            _qr.OnRead += Qr_OnRead;
            _qr.Start();

            return _loginToken.Token();
        }

        /// <summary>
        /// Opens Qr reader view.
        /// </summary>
        private void OpenQrReader()
        {
            _ui
                .Open<SessionQrViewController>(new UIReference
                {
                    UIDataId = "Qr.Session"
                }, out _qrId)
                .OnSuccess(el =>
                {
                    _view = el;
                })
                .OnFailure(ex => Log.Error(this, "Could not open Qr.Session : {0}.", ex));
        }

        /// <summary>
        /// Closes QR interface, stops reading from video.
        /// </summary>
        private void CloseQrReader()
        {
            _ui.Close(_qrId);

            // shutdown qr
            _qr.Stop();
            _qr.OnRead -= Qr_OnRead;
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

            var loginData = ParseQrLogin(value);
            if (!IsValidLogin(loginData))
            {
                _view.ShowMessage("Invalid QR code. Look at QR code in Enklu mobile app.");

                Log.Warning(this, "Invalid QR code value : {0}.", value);

                return;
            }

            // Valid Login Data, Close QR
            CloseQrReader();

            // make the call
            _holoAuthToken = _api
                .AuthMobileHoloLens
                .HololensMobileSignin(new Request
                {
                    UserId = loginData.UserId,
                    Code = loginData.Code
                });

            _holoAuthToken
                .OnSuccess(response =>
                {
                    if (response.Payload.Success)
                    {
                        var token = response.Payload.Body.AuthToken;
                        if (string.IsNullOrEmpty(token))
                        {
                            _view.ShowMessage("Could not login. Stargazer login failed. Please contact support@enklu.com if this persists.");
                            _loginToken.Fail(new Exception("Null token"));
                            return;
                        }

                        // Stargazer Credentials
                        var stargazerCreds = new StargazerCredentials
                        {
                            UserId = loginData.UserId,
                            Token = token
                        };

                        Log.Info(this, "Stargazer QR Login complete: {0}, {1}", loginData.UserId, token);

                        _loginToken.Succeed(stargazerCreds);
                    }
                    else
                    {
                        _view.ShowMessage("Could not login. Invalid QR code. Please contact support@enklu.com if this persists.");
                        _loginToken.Fail(new Exception("Failed to login with QR data."));
                    }
                })
                .OnFailure(exception =>
                {
                    Log.Error(this, "Could not sign into stargazer with QR code: {0}", exception);

                    _view.ShowMessage("Network error. Are you sure you're connected to the Internet? Please contact support@enklu.com if this persists.");
                    _loginToken.Fail(exception);
                });
        }

        /// <summary>
        /// Check for a valid login data tuple
        /// </summary>
        private bool IsValidLogin(StargazerHoloAuthPayload loginData)
        {
            return null != loginData 
               && !string.IsNullOrEmpty(loginData.UserId) 
               && !string.IsNullOrEmpty(loginData.Code);
        }

        /// <summary>
        /// Parses a user id and login code from Qr payload
        /// </summary>
        private StargazerHoloAuthPayload ParseQrLogin(string encoded)
        {
            var bytes = Encoding.UTF8.GetBytes(encoded);
            object instance;
            _json.Deserialize(typeof(JObject), ref bytes, out instance);

            var payload = (JObject) instance;
            var version = payload.Value<int>("v");
            var loginPayload = payload.Value<JObject>("p");

            switch (version)
            {
                case 1: return ParseLoginV1(loginPayload);
            }

            return null;
        }

        /// <summary>
        /// Parses version 1 of the QR payload.
        /// </summary>
        private StargazerHoloAuthPayload ParseLoginV1(JObject payload)
        {
            return new StargazerHoloAuthPayload(payload.Value<string>("u"), payload.Value<string>("c"));
        }

        /// <summary>
        /// Clear stargazer credentials from http service.
        /// </summary>
        private void ClearHttpCredentials()
        {
            _http.Services.Urls.Formatter("stargazer").Replacements.Remove("userId");
            _http.Services.RemoveHeader("stargazer", "Authorization");
        }

        /// <summary>
        /// Dispatch session created.
        /// </summary>
        private void DispatchCreated()
        {
            if (null != OnSessionCreated)
            {
                OnSessionCreated(_currentSession);
            }
        }

        /// <summary>
        /// Dispatch session ended.
        /// </summary>
        private void DispatchEnded()
        {
            if (null != OnSessionEnded)
            {
                OnSessionEnded();
            }
        }
    }
}