using System;
using System.Collections;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using CreateAR.Trellis.Messages;
using CreateAR.Trellis.Messages.EmailSignIn;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Application state that just prompts for login.
    /// </summary>
    public class InputLoginApplicationState : IState
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
        /// Messages.
        /// </summary>
        private readonly IMessageRouter _messages;

        /// <summary>
        /// Http service.
        /// </summary>
        private readonly IHttpService _http;
        
        /// <summary>
        /// Application wide config.
        /// </summary>
        private readonly ApplicationConfig _config;
        
        /// <summary>
        /// Api calls.
        /// </summary>
        private readonly ApiController _api;

        /// <summary>
        /// Controls input.
        /// </summary>
        private InputLoginController _inputController;

        /// <summary>
        /// Apps.
        /// </summary>
        private LoadAppController _apps;
    
        /// <summary>
        /// Constructor.
        /// </summary>
        public InputLoginApplicationState(
            IBootstrapper bootstrapper,
            IMessageRouter messages,
            IHttpService http,
            ApplicationConfig config,
            ApiController api)
        {
            _bootstrapper = bootstrapper;
            _messages = messages;
            _http = http;
            _config = config;
            _api = api;
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
            
        }

        /// <inheritdoc />
        public void Exit()
        {
            _inputController.gameObject.SetActive(false);
            _apps.gameObject.SetActive(false);
            
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

            Log.Info(this, "Loaded input login scene.");

            var root = GameObject.Find("InputLogin");

            _inputController = root.GetComponentInChildren<InputLoginController>(true);
            _inputController.OnSubmit += Controller_OnSubmit;
            _inputController.gameObject.SetActive(true);

            _apps = root.GetComponentInChildren<LoadAppController>(true);
            _apps.OnAppSelected += Controller_AppSelected;
        }

        /// <summary>
        /// Called when app has been selected.
        /// </summary>
        /// <param name="appId">The id of the app.</param>
        private void Controller_AppSelected(string appId)
        {
            // fill out app data
            _config.Play.AppId = appId;

            // load app
            _messages.Publish(MessageTypes.AR_SETUP);
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
                        var creds = _config.Network.Credentials(_config.Network.Current);
                        creds.UserId = response.Payload.Body.User.Id;
                        creds.Token = response.Payload.Body.Token;
                        creds.Apply(_http);

                        // deactivate
                        _inputController.gameObject.SetActive(false);
                        _apps.gameObject.SetActive(true);
                        
                        // get my apps
                        _api
                            .Apps
                            .GetMyApps()
                            .OnSuccess(appsResponse =>
                            {
                                if (appsResponse.Payload.Success)
                                {
                                    _apps.Show(appsResponse.Payload.Body);
                                }
                                else
                                {
                                    Log.Error(this,
                                        "There was an error getting my apps : {0}.",
                                        appsResponse.Payload.Error);
                                }
                            })
                            .OnFailure(exception => Log.Error(this, "Could not get apps : {0}.", exception));
                    }
                    else
                    {
                        Log.Error(this, "There was an error signing in : {0}.", response.Payload.Error);
                    }
                })
                .OnFailure(exception => Log.Error(this, "Could not signin : {0}.", exception));
        }
    }
}