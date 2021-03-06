using System;
using System.Linq;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using CreateAR.EnkluPlayer.IUX;
using CreateAR.EnkluPlayer.Scripting;
using CreateAR.Trellis.Messages;
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
        /// Handles.
        /// </summary>
	    private CrashService _crashService;

	    /// <summary>
	    /// The application to run.
	    /// </summary>
	    private Application _app;

        /// <summary>
        /// Times how long it takes to create Application.
        /// </summary>
	    private int _initTimer;

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
            
            // start timer
	        _initTimer = _binder.GetInstance<IMetricsService>().Timer(MetricsKeys.APPLICATION_INIT).Start();
            
            // watch for crashes
            _crashService = new CrashService(
                _binder.GetInstance<IMessageRouter>(),
                _binder.GetInstance<ApplicationConfig>(),
                _binder.GetInstance<ApiController>(),
                _binder.GetInstance<RuntimeStats>());
            _crashService.Startup();

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
            if (config.Enabled)
            {
                var targets = config.Targets.Split(',');
                if (targets.Contains("HostedGraphite") && !DeviceHelper.IsWebGl())
                {
                    Log.Info(this, "Adding HostedGraphiteMetricsTarget.");

                    var target = _binder.GetInstance<IHostedGraphiteMetricsTarget>();
                    try
                    {
                        target.Setup(config.Hostname, config.ApplicationKey);
                        metrics.AddTarget(target);
                    }
                    catch (Exception ex)
                    {
                        Log.Warning(this, "Could not setup HostedGraphiteMetricsTarget: {0}", ex);
                    }
                }

                if (targets.Contains(FileMetricsTarget.TYPE))
                {
                    Log.Info(this, "Adding FileMetricsTarget.");

                    metrics.AddTarget(new FileMetricsTarget());
                }
            }
            
            // handle restarts
            _binder.GetInstance<IMessageRouter>().Subscribe(
                MessageTypes.RESTART,
                _ =>
                {
                    _app.Uninitialize();
                    _app.Initialize();
                });
            
            // init app
            _app.Initialize();

            // stop timer
            metrics.Timer(MetricsKeys.APPLICATION_INIT).Stop(_initTimer);
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
	        Log.Info(this, "OnApplicationQuit.");

#if UNITY_EDITOR || UNITY_IOS
            var bridge = _binder.GetInstance<IBridge>() as WebSocketBridge;
	        if (null != bridge)
	        {
                Log.Info(this, "Disposing IBridge.");

	            bridge.Dispose();
	        }
#endif

	        var multiplayer = _binder.GetInstance<IMultiplayerController>();
            multiplayer.Disconnect();

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

	        _crashService.Shutdown();
        }
    }
}