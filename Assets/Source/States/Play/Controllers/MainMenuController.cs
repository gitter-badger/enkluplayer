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
        public ButtonWidget BtnPlay { get; private set; }

        [InjectElements("..btn-new")]
        public ButtonWidget BtnNew { get; private set; }

        [InjectElements("..btn-clearall")]
        public ButtonWidget BtnClearAll { get; private set; }

        [InjectElements("..btn-quit")]
        public ButtonWidget BtnQuit { get; private set; }

        [InjectElements("..toggle-debugrender")]
        public ToggleWidget ToggleDebugRender { get; private set; }

        /// <summary>
        /// Called when we wish to go back.
        /// </summary>
        public event Action OnBack;
        
        /// <summary>
        /// Called when the play button is pressed.
        /// </summary>
        public event Action OnPlay;

        /// <summary>
        /// Called when the new button is pressed.
        /// </summary>
        public event Action OnNew;

        /// <summary>
        /// Called when the clearall button is pressed.
        /// </summary>
        public event Action OnClearAll;

        /// <summary>
        /// Called when the quit button is pressed.
        /// </summary>
        public event Action OnQuit;

        /// <summary>
        /// Called when the DebugRender button is pressed.
        /// </summary>
        public event Action<bool> OnDebugRender;

        /// <inheritdoc />
        protected override void Awake()
        {
            base.Awake();

            Menu.OnBack += _ =>
            {
                if (null != OnBack)
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

            BtnQuit.Activator.OnActivated += _ =>
            {
                if (OnQuit != null)
                {
                    OnQuit();
                }
            };

            ToggleDebugRender.OnValueChanged += _ =>
            {
                if (OnDebugRender != null)
                {
                    OnDebugRender(ToggleDebugRender.Value);
                }
            };
        }
    }
}