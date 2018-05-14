﻿using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Trellis.Messages;
using CreateAR.Trellis.Messages.EmailSignIn;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Application state that just prompts for login.
    /// </summary>
    public class InputLoginStrategy : ILoginStrategy
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
        /// Constructor.
        /// </summary>
        public InputLoginStrategy(
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
            _loginToken.OnFinally(_ => frame.Release());
            
            OpenLogin();
            
            return _loginToken.Token();
        }

        /// <summary>
        /// Opens login view.
        /// </summary>
        private void OpenLogin()
        {
            _ui
                .Open<InputLoginUIView>(new UIReference
                {
                    UIDataId = "Login.Input"
                })
                .OnSuccess(el =>
                {
                    _loginView = el;
                    _loginView.OnSubmit += View_OnSubmit;
                    _loginView.OnSignUp += View_OnSignup;
                })
                .OnFailure(ex => Log.Error(this, "Could not open Login.Input : {0}.", ex));
        }

        /// <summary>
        /// Called when the view controller submit button has been pressed.
        /// </summary>
        /// <param name="username">Username.</param>
        /// <param name="password">Password.</param>
        private void View_OnSubmit(string username, string password)
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

                        _ui.Pop();
                        _loginView.Error.text = response.Payload.Error;
                    }
                })
                .OnFailure(exception =>
                {
                    Log.Error(this, "Could not signin : {0}.", exception);

                    _ui.Pop();
                    _loginView.Error.text = "Could not sign in. Please try again.";
                });
        }
        
        /// <summary>
        /// Called when the input view has called submit.
        /// </summary>
        private void View_OnSignup()
        {
            _ui.Pop();
            _ui
                .Open<MobileSignupUIView>(new UIReference
                {
                    UIDataId = "Signup"
                })
                .OnSuccess(el =>
                {
                    _signupView = el;
                    _signupView.OnSignUp += Mobile_OnSignUp;
                    _signupView.OnLicenseInfo += Mobile_OnLicenseInfo;
                })
                .OnFailure(exception => Log.Error(this, "Could not open mobile signup view."));
        }

        /// <summary>
        /// Called whebn license info button is pressed.
        /// </summary>
        private void Mobile_OnLicenseInfo()
        {
            _ui.Pop();
            _ui
                .Open<MobileLicenseUIView>(new UIReference
                {
                    UIDataId = "Signup.License"
                })
                .OnSuccess(el =>
                {
                    el.OnCancel += () =>
                    {
                        _ui.Pop();
                        
                        OpenLogin();
                    };
                    el.OnRequest += License_OnSubmit;
                })
                .OnFailure(exception => Log.Error(this, "Could not open Signup.License : {0}", exception));
        }

        /// <summary>
        /// Called when license UI is ready to submit info.
        /// </summary>
        /// <param name="data">Data to make request with.</param>
        private void License_OnSubmit(MobileLicenseUIView.LicenseRequestData data)
        {
            _ui.Pop();
            _ui.Open<IUIElement>(new UIReference
            {
                UIDataId = UIDataIds.LOADING
            });
            
            _api
                .GettingStarteds
                .GetStarted(new Trellis.Messages.GetStarted.Request
                {
                    Email = data.Email,
                    Name = data.Name,
                    Company = data.Company,
                    Phone = data.Phone,
                    Story = data.Story
                })
                .OnSuccess(response =>
                {
                    _ui.Pop();
                    _ui
                        .Open<MobileMessageUIView>(new UIReference
                        {
                            UIDataId = "Common.Message"
                        })
                        .OnSuccess(el =>
                        {
                            el.Title = "Thank you";
                            el.Description =
                                "Your request has been sent along. We should be getting back to you shortly. Thanks for your patience.";
                            el.Action = "Ok";
                            el.OnOk += () =>
                            {
                                _ui.Pop();
                                
                                OpenLogin();
                            };
                        })
                        .OnFailure(exception =>
                        {
                            Log.Error(this, "Could not open MobileMessageUIView : {0}", exception);
                            
                            OpenLogin();
                        });
                })
                .OnFailure(exception =>
                {
                    _ui.Pop();
                    _ui
                        .Open<ICommonErrorView>(new UIReference
                        {
                            UIDataId = UIDataIds.ERROR
                        })
                        .OnSuccess(el =>
                        {
                            el.Message = exception.Message;
                            el.OnOk += OpenLogin;
                        })
                        .OnFailure(ex => Log.Error(this, "Could not open error view : {0}", ex));
                });
        }

        /// <summary>
        /// Called when the signup view has called submit.
        /// </summary>
        /// <param name="data">The data to make the request with.</param>
        private void Mobile_OnSignUp(MobileSignupUIView.SignupRequestData data)
        {
            // show loading
            _ui.Open<IUIElement>(new UIReference
            {
                UIDataId = UIDataIds.LOADING
            });

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
                    _ui.Pop();

                    _signupView.Error.text = exception.Message;
                });
        }
    }
}