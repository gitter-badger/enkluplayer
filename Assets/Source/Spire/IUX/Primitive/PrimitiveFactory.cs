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
        /// <param name="widget"></param>
        /// <returns></returns>
        public IText LoadText(IWidget widget)
        {
            return (IText)Load(Instantiate(TextMonoBehaviour), widget);
        }

        /// <summary>
        /// Creates a text primitive.
        /// </summary>
        /// <param name="widget"></param>
        /// <returns></returns>
        public IActivator LoadActivator(IWidget widget)
        {
            return (IActivator)Load(Instantiate(ActivatorMonoBehaviour), widget);
        }

        /// <summary>
        /// Creates a text primitive.
        /// </summary>
        /// <param name="widget"></param>
        /// <returns></returns>
        public IReticle LoadReticle(IWidget widget)
        {
            return (IReticle)Load(Instantiate(ReticleMonoBehaviour), widget);
        }

        /// <summary>
        /// Initialization
        /// </summary>
        /// <returns></returns>
        private IPrimitive Load(IPrimitive primitive, IWidget widget)
        {
            primitive.Load(widget);
            return primitive;
        }
    }
}
