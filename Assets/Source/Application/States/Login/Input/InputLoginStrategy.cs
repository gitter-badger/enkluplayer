using System.Collections;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Trellis.Messages;
using CreateAR.Trellis.Messages.EmailSignIn;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Application state that just prompts for login.
    /// </summary>
    public class InputLoginStrategy : ILoginStrategy
    {
        /// <summary>
        /// Api calls.
        /// </summary>
        private readonly ApiController _api;

        /// <summary>
        /// Controls input.
        /// </summary>
        private InputLoginController _inputController;
        
        /// <summary>
        /// Tracks login internally.
        /// </summary>
        private AsyncToken<CredentialsData> _loginToken;
    
        /// <summary>
        /// Constructor.
        /// </summary>
        public InputLoginStrategy(ApiController api)
        {
            _api = api;
        }

        /// <inheritdoc />
        public IAsyncToken<CredentialsData> Login()
        {
            _loginToken = new AsyncToken<CredentialsData>();
            _loginToken.OnFinally(_ =>
            {
                _inputController.gameObject.SetActive(false);
            });
            
            var root = GameObject.Find("InputLogin");

            _inputController = root.GetComponentInChildren<InputLoginController>(true);
            _inputController.OnSubmit += Controller_OnSubmit;
            _inputController.gameObject.SetActive(true);

            return _loginToken.Token();
        }
        /// <summary>
        /// Called when the view controller submit button has been pressed.
        /// </summary>
        /// <param name="username">Username.</param>
        /// <param name="password">Password.</param>
        private void Controller_OnSubmit(string username, string password)
        {
            _api
                .EmailAuths
                .EmailSignIn(new Request
                {
                    Email = username,
                    Password = password
                })
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

                        _inputController.Error.text = response.Payload.Error;
                    }
                })
                .OnFailure(exception =>
                {
                    Log.Error(this, "Could not signin : {0}.", exception);

                    _inputController.Error.text = "Could not sign in. Please try again.";
                });
        }
    }
}