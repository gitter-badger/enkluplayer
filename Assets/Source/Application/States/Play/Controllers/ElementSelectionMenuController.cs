using System;
using CreateAR.SpirePlayer.IUX;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Manages the splash menu.
    /// </summary>
    [InjectVine("Element.Select")]
    public class ElementSelectionMenuController : InjectableIUXController
    {
        /// <summary>
        /// Button button.
        /// </summary>
        public ButtonWidget Btn
        {
            get { return (ButtonWidget) Root; }
        }

        /// <summary>
        /// Called when selected.
        /// </summary>
        public event Action OnSelected;

        /// <inheritdoc />
        protected override void Awake()
        {
            base.Awake();

            Btn.Activator.OnActivated += Btn_OnActivated;
        }

        /// <summary>
        /// Called when the button has been activated.
        /// </summary>
        /// <param name="activatorPrimitive">Activator primitive.</param>
        private void Btn_OnActivated(ActivatorPrimitive activatorPrimitive)
        {
            if (null != OnSelected)
            {
                OnSelected();
            }
        }
    }
}