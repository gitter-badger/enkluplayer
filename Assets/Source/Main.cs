using CreateAR.Commons.Unity.DebugRenderer;
using CreateAR.Commons.Unity.Logging;
using strange.extensions.injector.impl;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
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
	        Log.AddLogTarget(new FileLogTarget(new DefaultLogFormatter(), "Application.log"));
	        Log.Filter = LogLevel.Debug;

	        // setup debug renderer
	        var host = Object.FindObjectOfType<DebugRendererMonoBehaviour>();
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
            _app.Initialize();
        }

        /// <summary>
        /// Update loop.
        /// </summary>
	    private void Update()
	    {
	        _app.Update(Time.deltaTime);
	    }
	}
}