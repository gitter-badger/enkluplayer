using System;
using CreateAR.SpirePlayer.IUX;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Controls the splash menu for a prop.
    /// </summary>
    [InjectVine("Prop.Splash")]
    public class PropSplashController : InjectableIUXController
    {
        [InjectElements("btn-splash")]
        public ButtonWidget BtnSplash { get; private set; }
        
        public event Action OnOpen;

        protected override void Awake()
        {
            base.Awake();
        }

        public void Initialize(PropData prop)
        {
            BtnSplash.Schema.Set("label", prop.AssetId);
            BtnSplash.Activator.OnActivated += Activator_OnActivated;
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