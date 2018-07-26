using System;
using CreateAR.SpirePlayer.IUX;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// UIview for orientation adjustment menu.
    /// </summary>
    public class HmdOrientationUIView : MonoBehaviourIUXController
    {
        /// <summary>
        /// Voice command manager.
        /// </summary>
        [Inject]
        public IVoiceCommandManager Voice { get; set; }

        /// <summary>
        /// Continue button.
        /// </summary>
        [InjectElements("..btn-continue")]
        public ButtonWidget BtnContinue { get; private set; }

        /// <summary>
        /// Called when continue has been activated.
        /// </summary>
        public event Action OnContinue;

        /// <inheritdoc />
        protected override void AfterElementsCreated()
        {
            base.AfterElementsCreated();

            Voice.Register("continue", Voice_OnContinue);
            BtnContinue.Activator.OnActivated += _ =>
            {
                if (null != OnContinue)
                {
                    OnContinue();
                }
            };
        }

        /// <inheritdoc />
        protected override void OnDestroy()
        {
            base.OnDestroy();

            Voice.Unregister("continue");
        }

        /// <summary>
        /// Called when the voice command is called.
        /// </summary>
        /// <param name="value">The value.</param>
        private void Voice_OnContinue(string value)
        {
            if (null != OnContinue)
            {
                OnContinue();
            }
        }
    }
}