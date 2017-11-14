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
        public TextPrimitive TextPrimitive;
        public ActivatorPrimitive ActivatorPrimitive;
        public ReticlePrimitive ReticlePrimitive;

        /// <summary>
        /// Creates a text primitive.
        /// </summary>
        /// <param name="widget"></param>
        /// <returns></returns>
        public ITextPrimitive LoadText(IWidget widget)
        {
            return (ITextPrimitive)Load(Instantiate(TextPrimitive), widget);
        }

        /// <summary>
        /// Creates a text primitive.
        /// </summary>
        /// <param name="widget"></param>
        /// <returns></returns>
        public IActivatorPrimitive LoadActivator(IWidget widget)
        {
            return (IActivatorPrimitive)Load(Instantiate(ActivatorPrimitive), widget);
        }

        /// <summary>
        /// Creates a text primitive.
        /// </summary>
        /// <param name="widget"></param>
        /// <returns></returns>
        public IReticlePrimitive LoadReticle(IWidget widget)
        {
            return (IReticlePrimitive)Load(Instantiate(ReticlePrimitive), widget);
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
