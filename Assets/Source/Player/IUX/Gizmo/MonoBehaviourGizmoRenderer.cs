using UnityEngine;

namespace CreateAR.EnkluPlayer.IUX
{
    /// <summary>
    /// MonoBehaviour based GizmoRenderer.
    /// </summary>
    public class MonoBehaviourGizmoRenderer : MonoBehaviour, IGizmoRenderer
    {
        /// <inheritdoc />
        public Element Element { get; private set; }

        /// <inheritdoc />
        public virtual void Initialize(Element element)
        {
            Element = element;

            var unityElement = element as IUnityElement;
            if (null != unityElement)
            {
                transform.SetParent(unityElement.GameObject.transform, false);
            }
        }

        /// <inheritdoc />
        public virtual void Uninitialize()
        {
            Destroy(gameObject);
        }
    }
}