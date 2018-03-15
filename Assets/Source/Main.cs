using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using strange.extensions.injector.impl;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Potential modes.
    /// </summary>
    public enum PlayMode
    {
        Null = -1,
        Player,
        Release,
        Tool
    }

    /// <summary>
    /// Entry point of the application.
    /// </summary>
	public class Main : MonoBehaviour
	{
	    /// <summary>
        /// IoC container.
        /// </summary>
        private static readonly InjectionBinder _binder = new InjectionBinder();

	    /// <summary>
	    /// The application to run.
	    /// </summary>
	    private Application _app;
        
        /// <summary>
        /// Injects bindings into an object.
        /// </summary>
	    public static void Inject(object @object)
	    {
	        _binder.injector.Inject(@object);
	    }
        
        /// <summary>
        /// Analogous to the main() function.
        /// </summary>
	    private void Awake()
	    {
            // for AOT platforms
            AotGenericTypeIncludes.Include();

            // always run
	        UnityEngine.Application.runInBackground = true;

            // setup logging
	        Log.Filter = LogLevel.Debug;

	        //if (UnityEngine.Application.platform != RuntimePlatform.WebGLPlayer)
	        {
	            Log.AddLogTarget(new UnityLogTarget(new DefaultLogFormatter
	            {
	                Level = false,
	                Timestamp = false,
	                TypeName = true
	            }));
            }

#if FALSE && NETFX_CORE
            Log.AddLogTarget(new UwpSocketLogger(
                "Spire",
                new System.Uri("ws://127.0.0.1:9999")));
#endif // NETFX_CORE

#if !UNITY_WEBGL
            Log.AddLogTarget(new FileLogTarget(
				new DefaultLogFormatter(),
                System.IO.Path.Combine(
                    UnityEngine.Application.persistentDataPath,
                    "Application.log")));
#endif // UNITY_WEBGL

            // setup debug renderer
            var host = FindObjectOfType<DebugRendererMonoBehaviour>();
	        if (null != host)
	        {
	            Render.Renderer = host.Renderer;
	        }
	        else
	        {
	            Log.Error(this, "Could not find DebugRenderer host.");
	        }

            // load bindings
            _binder.Load(new SpirePlayerModule());

            // create application!
            _app = _binder.GetInstance<Application>();

	        if (null != _app)
	        {
	            Log.Info(this, "Application created.");
	        }
	        else
	        {
	            Log.Fatal(this, "Application could not be created.");
	        }
	    }

        /// <summary>
        /// Starts the application.
        /// </summary>
	    private void Start()
        {
            // handle restarts
            _binder.GetInstance<IMessageRouter>().Subscribe(
                MessageTypes.RESTART,
                _ =>
                {
                    _app.Uninitialize();
                    _app.Initialize();
                });

            _app.Initialize();
        }

        /// <summary>
        /// Update loop.
        /// </summary>
	    private void Update()
	    {
	        _app.Update(Time.deltaTime);
	    }

        /// <summary>
        /// Called when the application quits.
        /// </summary>
	    private void OnApplicationQuit()
	    {
#if UNITY_EDITOR || UNITY_IOS
	        var bridge = _binder.GetInstance<IBridge>() as WebSocketBridge;
	        if (null != bridge)
	        {
                Log.Info(this, "Disposing websocket server.");

	            bridge.Dispose();
	        }
#endif

            // clean up loggers
	        var targets = Log.Targets;
            for (var i = targets.Length - 1; i >= 0; i--)
            {
                var target = targets[i];
                Log.RemoveLogTarget(target);

	            var disposable = target as System.IDisposable;
	            if (null != disposable)
	            {
	                disposable.Dispose();
	            }
	        }
        }
	}
}