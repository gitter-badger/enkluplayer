using System;
using CreateAR.SpirePlayer.IUX;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Manages the splash menu.
    /// </summary>
    [InjectVine("Design.Splash")]
    public class SplashMenuController : InjectableIUXController
    {
        /// <summary>
        /// Main menu button.
        /// </summary>
        public ButtonWidget BtnMenu
        {
            get { return (ButtonWidget) Root; }
        }
        
        /// <summary>
        /// Called when the main menu should be opened.
        /// </summary>
        public event Action OnOpenMenu;

        /// <inheritdoc />
        protected override void Awake()
        {
            base.Awake();

            BtnMenu.Activator.OnActivated += Btn_OnActivated;
        }

        /// <summary>
        /// Called when the menu button has been activated.
        /// </summary>
        /// <param name="activatorPrimitive">Activator primitive.</param>
        private void Btn_OnActivated(ActivatorPrimitive activatorPrimitive)
        {
            if (null != OnOpenMenu)
            {
                OnOpenMenu();
            }
        }
    }
}