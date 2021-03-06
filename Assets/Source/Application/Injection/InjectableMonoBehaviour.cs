using UnityEngine;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Base class for MonoBehaviours that need to have bindings injected into
    /// them.
    /// </summary>
    public class InjectableMonoBehaviour : MonoBehaviour
    {
        /// <inheritdoc cref="MonoBehaviour"/>
        protected virtual void Awake()
        {
            Main.Inject(this);
        }
    }
}