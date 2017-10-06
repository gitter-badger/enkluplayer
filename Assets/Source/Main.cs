using CreateAR.Commons.Unity.Logging;
using strange.extensions.injector.impl;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Potential modes.
    /// </summary>
    public enum PlayMode
    {
        Player,
        Release
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
        /// 
        /// </summary>
	    public PlayMode Mode;

        /// <summary>
        /// Injects bindings into an object.
        /// </summary>
	    public static void Inject(Object @object)
	    {
	        _binder.injector.Inject(@object);
	    }
        
        /// <summary>
        /// Analogous to the main() function.
        /// </summary>
	    private void Awake()
	    {
            // setup logging
	        Log.AddLogTarget(new UnityLogTarget(new DefaultLogFormatter
	        {
	            Level = false,
	            Timestamp = false
	        }));
	        
	        Log.Filter = LogLevel.Debug;
            
#if NETFX_CORE
            Log.AddLogTarget(new UwpSocketLogger(
                "Spire",
                new System.Uri("ws://127.0.0.1:9999")));
#else       
            Log.AddLogTarget(new FileLogTarget(new DefaultLogFormatter(), "Application.log"));
#endif

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
            _binder.Load(new SpirePlayerModule(Mode));

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
#if UNITY_EDITOR
	        var bridge = _binder.GetInstance<IBridge>() as EditorBridge;
	        if (null != bridge)
	        {
                Log.Info(this, "Disposing websocket server.");

	            bridge.Dispose();
	        }
#endif

#if NETFX_CORE
	        foreach (var target in Log.Targets)
	        {
	            var disposable = target as System.IDisposable;
	            if (null != disposable)
	            {
	                disposable.Dispose();
	            }
	        }
#endif
        }
	}
}