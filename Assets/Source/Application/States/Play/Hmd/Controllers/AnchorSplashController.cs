using System;
using CreateAR.SpirePlayer.IUX;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Controls the splash menu for anchors.
    /// </summary>
    [InjectVine("Anchor.Splash")]
    public class AnchorSplashController : InjectableIUXController
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

        /// <inheritdoc />
        protected override void Awake()
        {
            base.Awake();

            BtnSplash.Activator.OnActivated += Activator_OnActivated;
        }

        /// <inheritdoc cref="MonoBehaviour" />
        private void Update()
        {
            var scale = transform.localScale;
            BtnSplash.Schema.Set(
                "scale",
                new Vec3(
                    1f / scale.x,
                    1f / scale.y,
                    1f / scale.z));
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