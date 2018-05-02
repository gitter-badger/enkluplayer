using System;
using System.Collections;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
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
        /// Name of the playmode scene to load.
        /// </summary>
        private const string SCENE_NAME = "InputLogin";
    
        /// <summary>
        /// Bootstraps coroutines.
        /// </summary>
        private readonly IBootstrapper _bootstrapper;

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
        public InputLoginStrategy(
            IBootstrapper bootstrapper,
            ApiController api)
        {
            _bootstrapper = bootstrapper;
            _api = api;
        }

        /// <inheritdoc />
        public IAsyncToken<CredentialsData> Login()
        {
            _loginToken = new AsyncToken<CredentialsData>();
            _loginToken.OnFinally(_ =>
            {
                _inputController.gameObject.SetActive(false);
                
                // unload scene
                SceneManager.UnloadSceneAsync(
                    SceneManager.GetSceneByName(SCENE_NAME));
            });

            // load scene
            _bootstrapper.BootstrapCoroutine(WaitForScene(
                SceneManager.LoadSceneAsync(
                    SCENE_NAME,
                    LoadSceneMode.Additive)));

            return _loginToken.Token();
        }
        
        /// <summary>
        /// Waits for scene to load.
        /// </summary>
        /// <param name="op">The scene load operation.</param>
        /// <returns></returns>
        private IEnumerator WaitForScene(AsyncOperation op)
        {
            yield return op;

            Log.Info(this, "Loaded input login scene.");

            var root = GameObject.Find("InputLogin");

            _inputController = root.GetComponentInChildren<InputLoginController>(true);
            _inputController.OnSubmit += Controller_OnSubmit;
            _inputController.gameObject.SetActive(true);
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

                        _loginToken.Fail(new Exception(response.Payload.Error));
                    }
                })
                .OnFailure(exception =>
                {
                    Log.Error(this, "Could not signin : {0}.", exception);

                    _loginToken.Fail(exception);
                });
        }
    }
}