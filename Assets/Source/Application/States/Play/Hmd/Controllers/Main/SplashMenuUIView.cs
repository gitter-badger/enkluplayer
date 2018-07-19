﻿using System;
using CreateAR.SpirePlayer.IUX;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Manages the splash menu.
    /// </summary>
    public class SplashMenuUIView : MonoBehaviourIUXController
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
        /// Main menu button.
        /// </summary>
        [InjectElements("btn-play")]
        public ButtonWidget BtnPlay { get; set; }

        /// <summary>
        /// Called when the main menu should be opened.
        /// </summary>
        public event Action OnOpenMenu;

        /// <summary>
        /// Called to go back to the user profile menu.
        /// </summary>
        public event Action OnBack;

        /// <summary>
        /// Called to move to play mode.
        /// </summary>
        public event Action OnPlay;
        
        /// <inheritdoc />
        protected override void AfterElementsCreated()
        {
            base.AfterElementsCreated();

            BtnMenu.Activator.OnActivated += Btn_OnActivated;
            BtnBack.Activator.OnActivated += BtnBack_OnActivated;
            BtnPlay.Activator.OnActivated += BtnPlay_OnActivated;
        }

        /// <summary>
        /// Called when the play button has been activated.
        /// </summary>
        /// <param name="activatorPrimitive">The activator.</param>
        private void BtnPlay_OnActivated(ActivatorPrimitive activatorPrimitive)
        {
            if (null != OnPlay)
            {
                OnPlay();
            }
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