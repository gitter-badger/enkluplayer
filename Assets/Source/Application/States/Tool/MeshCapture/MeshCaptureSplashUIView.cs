using System;
using CreateAR.SpirePlayer.IUX;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// UI for mesh capture.
    /// </summary>
    public class MeshCaptureSplashUIView : MonoBehaviourIUXController
    {
        /// <summary>
        /// Elements.
        /// </summary>
        [InjectElements("..btn-back")]
        public ButtonWidget BtnBack { get; set; }

        /// <summary>
        /// Called when back is requested.
        /// </summary>
        public event Action OnBack;

        /// <inheritdoc />
        protected override void AfterElementsCreated()
        {
            base.AfterElementsCreated();

            BtnBack.Activator.OnActivated += _ =>
            {
                if (null != OnBack)
                {
                    OnBack();
                }
            };
        }
    }
}