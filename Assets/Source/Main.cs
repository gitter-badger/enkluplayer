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
        /// Injects bindings into a MonoBehaviour.
        /// </summary>
        /// <param name="monobehaviour">The target MonoBehaviour.</param>
	    public static void Inject(InjectableMonoBehaviour monobehaviour)
	    {
	        _binder.injector.Inject(monobehaviour);
	    }

        /// <summary>
        /// Analogous to the main() function.
        /// </summary>
	    private void Awake()
	    {
            _binder.Load(new SpirePlayerModule());

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

	        Log.Info(this, "Application initialized.");
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