using System;
using CreateAR.SpirePlayer.IUX;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Menu for anchors.
    /// </summary>
    [InjectVine("Anchors.Menu")]
    public class AnchorMenuController : InjectableIUXController
    {
        /// <summary>
        /// Elements.
        /// </summary>
        public MenuWidget Menu
        {
            get { return (MenuWidget) Root; }
        }

        [InjectElements("..btn-new")]
        public ButtonWidget BtnNew { get; private set; }

        [InjectElements("..toggle-showchildren")]
        public ToggleWidget ToggleShowChildren { get; private set; }

        /// <summary>
        /// Called when the back button has been pressed.
        /// </summary>
        public event Action OnBack;

        /// <summary>
        /// Called when the new button has been pressed.
        /// </summary>
        public event Action OnNew;

        /// <summary>
        /// Called when showChildren has changed.
        /// </summary>
        public event Action<bool> OnShowChildrenChanged;

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

            BtnNew.Activator.OnActivated += _ =>
            {
                if (null != OnNew)
                {
                    OnNew();
                }
            };

            ToggleShowChildren.OnValueChanged += _ =>
            {
                if (null != OnShowChildrenChanged)
                {
                    OnShowChildrenChanged(ToggleShowChildren.Value);
                }
            };
        }
    }
}