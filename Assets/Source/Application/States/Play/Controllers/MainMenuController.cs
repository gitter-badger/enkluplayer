using System;
using CreateAR.SpirePlayer.IUX;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Manages the main menu.
    /// </summary>
    [InjectVine("Design.MainMenu")]
    public class MainMenuController : InjectableIUXController
    {
        /// <summary>
        /// Elements.
        /// </summary>
        public MenuWidget Menu
        {
            get { return (MenuWidget) Root; }
        }

        [InjectElements("..btn-play")]
        public ButtonWidget BtnPlay { get; set; }

        [InjectElements("..btn-new")]
        public ButtonWidget BtnNew { get; set; }

        [InjectElements("..btn-anchors")]
        public ButtonWidget BtnAnchors { get; set; }

        [InjectElements("..btn-clearall")]
        public ButtonWidget BtnClearAll { get; set; }
        
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
        public event Action OnShowAnchorMenu;

        /// <summary>
        /// Called when the new button is pressed.
        /// </summary>
        public event Action OnNew;

        /// <summary>
        /// Called when the clearall button is pressed.
        /// </summary>
        public event Action OnClearAll;
        
        /// <inheritdoc />
        protected override void Awake()
        {
            base.Awake();

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

            BtnAnchors.Activator.OnActivated += _ =>
            {
                if (OnShowAnchorMenu != null)
                {
                    OnShowAnchorMenu();
                }
            };
            
            BtnNew.Activator.OnActivated += _ =>
            {
                if (OnNew != null)
                {
                    OnNew();
                }
            };

            BtnClearAll.Activator.OnActivated += _ =>
            {
                if (OnClearAll != null)
                {
                    OnClearAll();
                }
            };
        }
    }
}