using System;
using CreateAR.SpirePlayer.IUX;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Base class for design mode controllers.
    /// </summary>
    public class ElementDesignController : MonoBehaviour
    {
        /// <summary>
        /// The element.
        /// </summary>
        public Element Element { get; private set; }

        /// <summary>
        /// Called when the controller is about to be destroyed. This exists
        /// so we can take care of uninitialization before the GameObject
        /// is no longer preset.
        /// </summary>
        public event Action<ElementDesignController> OnDestroyed;

        /// <summary>
        /// Initializes the controller.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="context">Context.</param>
        public virtual void Initialize(Element element, object context)
        {
            Element = element;
        }

        /// <summary>
        /// Uninitializes the controller.
        /// </summary>
        public virtual void Uninitialize()
        {
            //
        }

        /// <inheritdoc cref="MonoBehaviour"/>
        protected virtual void OnDestroy()
        {
            if (null != OnDestroyed)
            {
                OnDestroyed(this);
            }
        }
    }
}