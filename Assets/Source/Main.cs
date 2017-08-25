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
	    }
	}
}