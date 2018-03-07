using System;
using CreateAR.SpirePlayer.IUX;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Menu for anchors.
    /// </summary>
    [InjectVine("Anchor.Menu")]
    public class AnchorMenuController : InjectableIUXController
    {
        /// <summary>
        /// Elements.
        /// </summary>
        [InjectElements("..btn-back")]
        public ButtonWidget BtnBack { get; private set; }

        [InjectElements("..btn-new")]
        public ButtonWidget BtnNew { get; private set; }

        /// <summary>
        /// Called when the back button has been pressed.
        /// </summary>
        public event Action OnBack;

        /// <summary>
        /// Called when the new button has been pressed.
        /// </summary>
        public event Action OnNew;
        
        /// <inheritdoc />
        protected override void Awake()
        {
            base.Awake();

            BtnBack.Activator.OnActivated += _ =>
            {
                if (null != OnBack)
                {
                    OnBack();
                }
            };

            BtnNew.Activator.OnActivated += _ =>
            {
                if (null != OnNew)
                {
                    OnNew();
                }
            };
        }
    }
}