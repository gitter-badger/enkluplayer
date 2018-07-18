using System;
using CreateAR.SpirePlayer.IUX;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Manages the main menu.
    /// </summary>
    public class MainMenuUIView : MonoBehaviourIUXController
    {
        /// <summary>
        /// Elements.
        /// </summary>
        [InjectElements("..menu")]
        public MenuWidget Menu { get; set; }

        [InjectElements("..btn-play")]
        public ButtonWidget BtnPlay { get; set; }
        
        [InjectElements("..btn-new-asset")]
        public ButtonWidget BtnNewAsset { get; set; }

        [InjectElements("..btn-new-anchor")]
        public ButtonWidget BtnNewAnchor { get; set; }

        [InjectElements("..btn-new-text")]
        public ButtonWidget BtnNewText { get; set; }

        [InjectElements("..btn-new-container")]
        public ButtonWidget BtnNewContainer { get; set; }

        [InjectElements("..btn-resetdata")]
        public ButtonWidget BtnResetData{ get; set; }

        /// <summary>
        /// Called when we wish to go back.
        /// </summary>
        public event Action OnBack;
        
        /// <summary>
        /// Called when the play button is pressed.
        /// </summary>
        public event Action OnPlay;

        /// <summary>
        /// Shows/hides anchors.
        /// </summary>
        public event Action OnShowSettings;

        /// <summary>
        /// Called when the new button is pressed.
        /// </summary>
        public event Action OnNew;
        
        /// <inheritdoc />
        protected override void AfterElementsCreated()
        {
            base.AfterElementsCreated();

            Menu.OnBack += _ =>
            {
                if (OnBack != null)
                {
                    OnBack();
                }
            };

            BtnPlay.Activator.OnActivated += _ =>
            {
                if (OnPlay != null)
                {
                    OnPlay();
                }
            };
        }
    }
}