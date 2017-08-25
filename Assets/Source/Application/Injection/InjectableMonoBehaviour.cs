using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Base class for MonoBehaviours that need to have bindings injected into
    /// them.
    /// </summary>
    public class InjectableMonoBehaviour : MonoBehaviour
    {
        /// <summary>
        /// Called first thing.
        /// </summary>
        private void Awake()
        {
            Main.Inject(this);
        }
    }
}