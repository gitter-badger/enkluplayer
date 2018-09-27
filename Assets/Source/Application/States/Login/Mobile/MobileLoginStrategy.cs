using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Trellis.Messages;
using CreateAR.Trellis.Messages.EmailSignIn;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Application state that just prompts for login.
    /// </summary>
    public class MobileLoginStrategy : ILoginStrategy
    {
        /// <summary>
        /// Manages UI.
        /// </summary>
        private readonly IUIManager _ui;
        
        /// <summary>
        /// Api calls.
        /// </summary>
        private readonly ApiController _api;

        /// <summary>
        /// Login view.
        /// </summary>
        private InputLoginUIView _loginView;

        /// <summary>
        /// Signup view.
        /// </summary>
        private MobileSignupUIView _signupView;
        
        /// <summary>
        /// Tracks login internally.
        /// </summary>
        private AsyncToken<CredentialsData> _loginToken;
        
        /// <summary>
        /// Id of login view.
        /// </summary>
        private int _loginViewId = -1;

        /// <summary>
        /// Id of registration view.
        /// </summary>
        private int _registrationViewId = -1;
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public MobileLoginStrategy(
            IUIManager ui,
            ApiController api)
        {
            _ui = ui;
            _api = api;
        }

        /// <inheritdoc />
        public IAsyncToken<CredentialsData> Login()
        {
            var frame = _ui.CreateFrame();
            
            _loginToken = new AsyncToken<CredentialsData>();
            _loginToken.OnSuccess(_ => frame.Release());
            _loginToken.OnFailure(_ => frame.Release());
            
            OpenRegistration();
            
            return _loginToken.Token();
        }

        /// <summary>
        /// Opens registration view.
        /// </summary>
        private void OpenRegistration()
        {
            if (!_ui.Reveal(_registrationViewId))
            {
                _ui
                    .Open<MobileSignupUIView>(new UIReference
                    {
                        UIDataId = "Signup"
                    }, out _registrationViewId)
                    .OnSuccess(el =>
                    {
                        _signupView = el;
                        _signupView.OnSubmit += SignUp_OnSubmit;
                        _signupView.OnLicenseInfo += SignUp_OnLicenseInfo;
                        _signupView.OnLogin += OpenLogin;
                        _signupView.OnContinueAsGuest += ContinueAsGuest;
                    })
                    .OnFailure(exception => Log.Error(this, "Could not open mobile signup view."));
            }
        }

        /// <summary>
        /// Opens login view.
        /// </summary>
        private void OpenLogin()
        {
            if (!_ui.Reveal(_loginViewId))
            {
                _ui
                    .Open<InputLoginUIView>(new UIReference
                    {
                        UIDataId = "Login.Input"
                    }, out _loginViewId)
                    .OnSuccess(el =>
                    {
                        _loginView = el;
                        _loginView.OnSubmit += Login_OnSubmit;
                        _loginView.OnSignUp += OpenRegistration;
                        _loginView.OnContinueAsGuest += ContinueAsGuest;
                    })
                    .OnFailure(ex => Log.Error(this, "Could not open Login.Input : {0}.", ex));   
            }
        }
        
        /// <summary>
        /// Called when user is continuing as a guest.
        /// </summary>
        private void ContinueAsGuest()
        {
            _loginToken.Succeed(new CredentialsData
            {
                UserId = "Guest"
            });
        }

        /// <summary>
        /// Called when the view controller submit button has been pressed.
        /// </summary>
        /// <param name="username">Username.</param>
        /// <param name="password">Password.</param>
        private void Login_OnSubmit(string username, string password)
        {
            int loadingId;
            _ui
                .Open<IUIElement>(new UIReference
                {
                    UIDataId = UIDataIds.LOADING
                }, out loadingId);
            
            _api
                .EmailAuths
                .EmailSignIn(new Request
                {
                    Email = username,
                    Password = password
                })
                .OnFinally(_ => _ui.Close(loadingId))
                .OnSuccess(response =>
                {
                    if (response.Payload.Success)
                    {
                        // fill out credentials
                        var creds = new CredentialsData
                        {
                            UserId = response.Payload.Body.User.Id,
                            Token = response.Payload.Body.Token
                        };
                        
                        _loginToken.Succeed(creds);
                    }
                    else
                    {
                        Log.Error(this, "There was an error signing in : {0}.", response.Payload.Error);

                        _loginView.Error.text = response.Payload.Error;
                    }
                })
                .OnFailure(exception =>
                {
                    Log.Error(this, "Could not signin : {0}.", exception);

                    _loginView.Error.text = "Could not sign in. Please try again.";
                });
        }
        
        /// <summary>
        /// Called when the signup view has called submit.
        /// </summary>
        /// <param name="data">The data to make the request with.</param>
        private void SignUp_OnSubmit(MobileSignupUIView.SignupRequestData data)
        {
            // show loading
            int loadingId;
            _ui.Open<IUIElement>(new UIReference
            {
                UIDataId = UIDataIds.LOADING
            }, out loadingId);

            Log.Info(this, "Submit : {0}.", data.LicenseKey);
            
            // make request
            _api
                .EmailAuths
                .EmailSignUpWithLicense(new Trellis.Messages.EmailSignUpWithLicense.Request
                {
                    DisplayName = data.DisplayName,
                    Email = data.Email,
                    LicenseKey = data.LicenseKey,
                    Password = data.Password
                })
                .OnFinally(_ => _ui.Close(loadingId))
                .OnSuccess(response =>
                {
                    _loginToken.Succeed(new CredentialsData
                    {
                        Email = data.Email,
                        UserId = response.Payload.Body.User.Id,
                        Token = response.Payload.Body.Token
                    });
                })
                .OnFailure(exception =>
                {
                    _signupView.Error.text = exception.Message;
                });
        }

        /// <summary>
        /// Called whebn license info button is pressed.
        /// </summary>
        private void SignUp_OnLicenseInfo()
        {
            int licenseViewId;
            _ui
                .Open<MobileLicenseUIView>(new UIReference
                {
                    UIDataId = "Signup.License"
                },
                out licenseViewId)
                .OnSuccess(el =>
                {
                    el.OnCancel += OpenRegistration;
                    //el.OnRequest += License_OnSubmit;
                })
                .OnFailure(exception => Log.Error(this, "Could not open Signup.License : {0}", exception));
        }
    }
}