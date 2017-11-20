using UnityEngine;

namespace CreateAR.SpirePlayer.UI
{
    /// <summary>
    /// Contains prefabs for UI rendering primitives used in all widgets.
    /// </summary>
    public class PrimitiveFactory : MonoBehaviour, IPrimitiveFactory
    {
        /// <summary>
        /// Basic text rendering primitive.
        /// </summary>
        public TextMonoBehaviour TextMonoBehaviour;
        public ActivatorMonoBehaviour ActivatorMonoBehaviour;
        public ReticleMonoBehaviour ReticleMonoBehaviour;

        /// <summary>
        /// Creates a text primitive.
        /// </summary>
        /// <returns></returns>
        public IText Text()
        {
            return Instantiate(TextMonoBehaviour);
        }

        /// <summary>
        /// Creates a text primitive.
        /// </summary>
        /// <returns></returns>
        public IActivator Activator()
        {
            return Instantiate(ActivatorMonoBehaviour);
        }

        /// <summary>
        /// Creates a text primitive.
        /// </summary>
        /// <returns></returns>
        public IReticle Reticle()
        {
            return Instantiate(ReticleMonoBehaviour);
        }        
    }
}
