using UnityEngine;

namespace CreateAR.SpirePlayer.UI
{
    /// <summary>
    /// Contains prefabs for UI rendering primitives used in all widgets.
    /// </summary>
    public class PrimitiveFactory : MonoBehaviour, IPrimitiveFactory
    {
        /// <summary>
        /// Basic text rendering primitive
        /// </summary>
        public TextPrimitive TextPrimitive;
        public ActivatorPrimitive ActivatorPrimitive;

        /// <summary>
        /// Creates a text primitive.
        /// </summary>
        /// <param name="parentTransform"></param>
        /// <returns></returns>
        public ITextPrimitive RequestText(Transform parentTransform)
        {
            return (ITextPrimitive)Load(Instantiate(TextPrimitive), parentTransform);
        }

        /// <summary>
        /// Creates a text primitive.
        /// </summary>
        /// <param name="parentTransform"></param>
        /// <returns></returns>
        public IActivatorPrimitive RequestActivator(Transform parentTransform)
        {
            return (IActivatorPrimitive)Load(Instantiate(ActivatorPrimitive), parentTransform);
        }

        /// <summary>
        /// TODO: pooling of primitive resources for reuse.
        /// </summary>
        /// <param name="primitive"></param>
        public void Release(IPrimitive primitive)
        {
            if (primitive != null
             && primitive.Transform != null)
            {
                Object.Destroy(primitive.Transform.gameObject);
            }
        }

        /// <summary>
        /// Loads the prefab's scheme and 
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="parentTransform"></param>
        /// <returns></returns>
        private IPrimitive Load(IPrimitive instance, Transform parentTransform)
        {
            instance.Transform.SetParent(parentTransform, false);
            instance.Transform.gameObject.SetActive(true);
            return instance;
        }
    }
}
