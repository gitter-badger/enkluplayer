using System;
using CreateAR.SpirePlayer.IUX;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// UI View for adjusting the primary anchor.
    /// </summary>
    public class AdjustPrimaryAnchorUIView : MonoBehaviourIUXController
    {
        /// <summary>
        /// The anchor we're adjusting.
        /// </summary>
        private AnchorDesignController _controller;
        
        /// <summary>
        /// Various injected elements.
        /// </summary>
        [InjectElements("..btn-back")]
        public ButtonWidget BtnBack { get; set; }
        [InjectElements("..tgl-scan")]
        public ToggleWidget TglScan { get; set; }
        [InjectElements("..tgl-show")]
        public ToggleWidget TglShow { get; set; }

        /// <summary>
        /// Called to go back.
        /// </summary>
        public event Action OnBack;

        /// <summary>
        /// Called when autoscan changes.
        /// </summary>
        public event Action<bool> OnAutoScanChanged;

        /// <summary>
        /// Called when visibility changes.
        /// </summary>
        public event Action<bool> OnVisibilityChanged;

        /// <summary>
        /// Initializes the menu.
        /// </summary>
        /// <param name="controller">The prop controller.</param>
        public void Initialize(AnchorDesignController controller)
        {
            _controller = controller;

            transform.position = _controller.transform.position;
        }

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

            TglScan.OnValueChanged += _ =>
            {
                if (null != OnAutoScanChanged)
                {
                    OnAutoScanChanged(TglScan.Value);
                }
            };

            TglShow.OnValueChanged += _ =>
            {
                if (null != OnVisibilityChanged)
                {
                    OnVisibilityChanged(TglShow.Value);
                }
            };
        }
    }
}