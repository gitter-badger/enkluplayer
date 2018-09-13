using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using CreateAR.EnkluPlayer.IUX;
using CreateAR.EnkluPlayer.Scripting;
using strange.extensions.injector.impl;
using UnityEngine;

namespace CreateAR.EnkluPlayer
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
        /// TODO: REMOVE THIS EVIL. This is here because of a circular dependency. IElementFactory < == > IElementJsFactory.
        /// </summary>
        /// <param name="jsCache">The cache.</param>
        /// <returns></returns>
	    public static AppJsApi NewAppJsApi(IElementJsCache jsCache)
	    {
            return new AppJsApi(
                new AppScenesJsApi(jsCache, _binder.GetInstance<IAppSceneManager>()),
                new AppElementsJsApi(
                    jsCache,
                    _binder.GetInstance<IElementFactory>(),
                    _binder.GetInstance<IElementManager>()),
                _binder.GetInstance<PlayerJs>());
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

            // log to unity only in the editor and webgl
#if !UNITY_WEBGL
            //if (UnityEngine.Application.isEditor)
#endif
            {
	            Log.AddLogTarget(new UnityLogTarget(new DefaultLogFormatter
	            {
	                Level = false,
	                Timestamp = false,
	                TypeName = true
	            }));
            }

            // non-webgl should log to file
#if !UNITY_WEBGL
            Log.AddLogTarget(new FileLogTarget(
				new DefaultLogFormatter
				{
                    Level = true,
                    Timestamp = true,
                    TypeName = true
				},
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
            _binder.Load(new EnkluPlayerModule());

	        // non-editor builds should log to loggly
	        if (!UnityEngine.Application.isEditor)
	        {
	            Log.AddLogTarget(new LogglyLogTarget(
	                "1f0810f5-db28-4ea3-aeea-ec83d8cb3c0f",
	                "EnkluPlayer",
	                _binder.GetInstance<ILogglyMetadataProvider>(),
	                _binder.GetInstance<IBootstrapper>())
	            {
                    // only log errors + above
                    Filter = LogLevel.Error
	            });
            }

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
            // start metrics
            var config = _binder.GetInstance<ApplicationConfig>().Metrics;
            var metrics = _binder.GetInstance<IMetricsService>();
            if (!UnityEngine.Application.isEditor)
            {
#if !UNITY_WEBGL
                metrics.AddTarget(new HostedGraphiteMetricsTarget(
                    config.Hostname,
                    config.ApplicationKey));
#endif
            }

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
		/// Called for application suspend/resume.
		/// </summary>
		/// <param name="status">True iff the application is paused.</param>
		private void OnApplicationPause(bool status)
		{
			if (status)
			{
				_app.Suspend();
			}
			else
			{
				_app.Resume();
			}
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
                Log.Info(this, "Disposing IBridge.");

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