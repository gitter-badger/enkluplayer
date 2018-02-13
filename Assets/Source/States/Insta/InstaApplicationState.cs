using System.Collections;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.IUX;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Simple application that loads a preauthored scene and allows the user
    /// to take a screenshot.
    /// </summary>
    public class InstaApplicationState : IState
    {
        /// <summary>
        /// Name of the playmode scene to load.
        /// </summary>
        private const string SCENE_NAME = "Insta";
        
        /// <summary>
        /// Bootstraps coroutines.
        /// </summary>
        private readonly IBootstrapper _bootstrapper;

        /// <summary>
        /// Elements.
        /// </summary>
        private readonly IElementFactory _elements;
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public InstaApplicationState(
            IBootstrapper bootstrapper,
            IElementFactory elements)
        {
            _bootstrapper = bootstrapper;
            _elements = elements;
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
            
            Log.Info(this, "Loaded Insta scene.");

            _elements.Element(@"<Content assetSrc='80a18c8a-a058-4d7a-a082-715636b58d6b' />");
        }
    }
}