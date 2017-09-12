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
        private readonly FiniteStateMachine _states;
        
        /// <summary>
        /// Dependencies.
        /// </summary>
        private readonly IMessageRouter _messages;
        private readonly IAssetManager _assets;
        private readonly IAssetUpdateService _assetUpdater;

        /// <summary>
        /// Creates a new Application.
        /// </summary>
        public Application(
            EditApplicationState edit,
            PreviewApplicationState preview,

            IMessageRouter messages,
            IAssetManager assets,
            IAssetUpdateService assetUpdater)
        {
            Log.AddLogTarget(new UnityLogTarget(new DefaultLogFormatter
            {
                Level = false,
                Timestamp = false
            }));
            Log.AddLogTarget(new FileLogTarget(new DefaultLogFormatter(), "Application.log"));
            Log.Filter = LogLevel.Debug;

            _states = new FiniteStateMachine(new IState[]
            {
                edit,
                preview
            });
            
            _messages = messages;
            _assets = assets;
            _assetUpdater = assetUpdater;

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

            // setup messages
            _messages.Subscribe(
                MessageTypes.PREVIEW_ASSET,
                (message, unsub) =>
                {
                    

                    _states.Change<PreviewApplicationState>();
                });

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
                    Queries = new StandardQueryResolver(),
                    Service = _assetUpdater
                })
                .OnSuccess(_ =>
                {
                    Log.Info(this, "AssetManager initialized.");

                    Ready();
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

        /// <summary>
        /// Called, internally, when the Application is ready.
        /// </summary>
        private void Ready()
        {
            // move to the default application state
            _states.Change<EditApplicationState>();
        }
    }
}