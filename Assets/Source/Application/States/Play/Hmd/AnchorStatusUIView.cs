using System;
using CreateAR.EnkluPlayer.IUX;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Displays anchor information.
    /// </summary>
    public class AnchorStatusUIView : MonoBehaviourIUXController
    {
        /// <summary>
        /// Elements.
        /// </summary>
        [InjectElements("..cpn-progress")]
        public TextWidget TxtProgress { get; set; }
        [InjectElements("..cpn-locating")]
        public TextWidget TxtLocating { get; set; }
        [InjectElements("..btn")]
        public ButtonWidget BtnBypass { get; set; }

        /// <summary>
        /// Called when bypass has been requested.
        /// </summary>
        public event Action OnBypass;
        
        /// <inheritdoc />
        protected override void AfterElementsCreated()
        {
            BtnBypass.OnActivated += _ =>
            {
                if (null != OnBypass)
                {
                    OnBypass();
                }
            };
        }
    }
}