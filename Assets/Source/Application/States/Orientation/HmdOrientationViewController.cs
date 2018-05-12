using System;
using CreateAR.SpirePlayer.IUX;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Manu for orientation adjustment menu.
    /// </summary>
    [InjectVine("Orientation.Adjust")]
    public class HmdOrientationViewController : InjectableIUXController
    {
        /// <summary>
        /// Voice command manager.
        /// </summary>
        [Inject]
        public IVoiceCommandManager Voice { get; set; }

        /// <summary>
        /// Called when continue has been activated.
        /// </summary>
        public event Action OnContinue;

        /// <inheritdoc />
        protected override void Awake()
        {
            base.Awake();

            Voice.Register("continue", Voice_OnContinue);
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