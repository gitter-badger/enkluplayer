﻿using System;
using CreateAR.EnkluPlayer.IUX;
using UnityEngine;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Base class for design mode controllers.
    /// </summary>
    public class ElementDesignController : MonoBehaviour
    {
        /// <summary>
        /// Transform of element.
        /// </summary>
        public Transform ElementTransform { get; private set; }

        /// <summary>
        /// The element.
        /// </summary>
        public Element Element { get; private set; }

        /// <summary>
        /// True iff element menu is visible.
        /// </summary>
        public virtual bool MenuVisible { get; set; }

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
            ElementTransform = ((IUnityElement) element).GameObject.transform;
        }
        
        /// <summary>
        /// Uninitializes the controller.
        /// </summary>
        public virtual void Uninitialize()
        {
            //
        }

        /// <inheritdoc cref="MonoBehaviour"/>
        protected virtual void Update()
        {
            // follow element
            transform.position = ElementTransform.position;
            transform.rotation = ElementTransform.rotation;
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