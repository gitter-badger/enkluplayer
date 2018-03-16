using System;
using CreateAR.SpirePlayer.IUX;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Controls the splash menu for content.
    /// </summary>
    [InjectVine("Content.Splash")]
    public class ContentSplashController : InjectableIUXController
    {
        /// <summary>
        /// Splash button.
        /// </summary>
        public ButtonWidget BtnSplash
        {
            get { return (ButtonWidget) Root; }
        }
        
        /// <summary>
        /// Called when open requested.
        /// </summary>
        public event Action OnOpen;
        
        /// <summary>
        /// Initiailizes the controller.
        /// </summary>
        /// <param name="propName">The name of the prop.</param>
        public void Initialize(string propName)
        {
            BtnSplash.Schema.Set("label", propName);
            BtnSplash.Activator.OnActivated += Activator_OnActivated;
        }

        /// <inheritdoc cref="MonoBehaviour" />
        private void Update()
        {
            // scale
            var scale = transform.lossyScale;
            var max = Mathf.Max(scale.x, scale.y, scale.z);
            BtnSplash.GameObject.transform.localScale = 1f / max * Vector3.one;
        }

        /// <summary>
        /// Called when the activator is activated.
        /// </summary>
        /// <param name="activatorPrimitive">The activator.</param>
        private void Activator_OnActivated(ActivatorPrimitive activatorPrimitive)
        {
            if (null != OnOpen)
            {
                OnOpen();
            }
        }
    }
}