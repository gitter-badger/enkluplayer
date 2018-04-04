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
        [InjectElements("btn-menu")]
        public ButtonWidget BtnMenu { get; set; }

        /// <summary>
        /// Main menu button.
        /// </summary>
        [InjectElements("btn-back")]
        public ButtonWidget BtnBack{ get; set; }

        /// <summary>
        /// Called when the main menu should be opened.
        /// </summary>
        public event Action OnOpenMenu;

        /// <summary>
        /// Called to go back to the user profile menu.
        /// </summary>
        public event Action OnBack;

        /// <inheritdoc />
        protected override void Awake()
        {
            base.Awake();

            BtnMenu.Activator.OnActivated += Btn_OnActivated;
            BtnBack.Activator.OnActivated += BtnBack_OnActivated;
        }

        /// <summary>
        /// Called when the back button has been actived.
        /// </summary>
        /// <param name="activatorPrimitive">The activator.</param>
        private void BtnBack_OnActivated(ActivatorPrimitive activatorPrimitive)
        {
            if (null != OnBack)
            {
                OnBack();
            }
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