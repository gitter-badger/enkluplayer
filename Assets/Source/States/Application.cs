using CreateAR.Commons.Unity.DebugRenderer;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Root application.
    /// </summary>
    public class Application
    {
        /// <summary>
        /// Controls application states.
        /// </summary>
        private readonly StateMachine _states = new StateMachine();
        
        /// <summary>
        /// Default state of the application.
        /// </summary>
        private readonly EditApplicationState _defaultState;

        /// <summary>
        /// Manages assets.
        /// </summary>
        private readonly IAssetManager _assets;

        /// <summary>
        /// Creates a new Application.
        /// </summary>
        public Application(
            EditApplicationState defaultState,
            IAssetManager assets)
        {
            Log.AddLogTarget(new UnityLogTarget(new DefaultLogFormatter
            {
                Level = false,
                Timestamp = false
            }));
            Log.AddLogTarget(new FileLogTarget(new DefaultLogFormatter(), "Application.log"));
            Log.Filter = LogLevel.Debug;

            _defaultState = defaultState;
            _assets = assets;

#if !UNITY_EDITOR && UNITY_WEBGL
            WebGLInput.captureAllKeyboardInput = false;
#endif
        }

        /// <summary>
        /// Initializes the application.
        /// </summary>
        public void Initialize()
        {
            // setup renderer
            var host = Object.FindObjectOfType<DebugRendererMonoBehaviour>();
            if (null != host)
            {
                Render.Renderer = host.Renderer;
            }
            else
            {
                Log.Error(this, "Could not find DebugRenderer host.");
            }

            // setup assets
            _assets
                .Initialize(new AssetManagerConfiguration
                {
                    Loader = new StandardAssetLoader(new UrlBuilder
                    {
                        BaseUrl = "ec2-54-202-152-140.us-west-2.compute.amazonaws.com",
                        Port = 9091,
                        Protocol = "http"
                    }),
                    Queries = new StandardQueryResolver()
                })
                .OnSuccess(_ =>
                {
                    Log.Info(this, "AssetManager initialized.");

                    // move to the default application state
                    _states.Change(_defaultState);
                })
                .OnFailure(exception =>
                {
                    // rethrow
                    throw exception;
                });
        }
        
        /// <summary>
        /// Called every frame.
        /// </summary>
        /// <param name="dt">The time since last time Update was called.</param>
        public void Update(float dt)
        {
            _states.Update(dt);
        }
    }
}