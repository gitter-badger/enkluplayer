using System;
using CreateAR.SpirePlayer.IUX;
using UnityEngine;

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
        [InjectElements("..btn-reset")]
        public ButtonWidget BtnReset { get; set; }
        [InjectElements("..tgl-scan")]
        public ToggleWidget TglScan { get; set; }
        [InjectElements("..tgl-show")]
        public ToggleWidget TglShow { get; set; }

        /// <summary>
        /// Called to go back.
        /// </summary>
        public event Action OnBack;

        /// <summary>
        /// Called to reset anchors.
        /// </summary>
        public event Action OnReset;

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
        public void Initialize(
            AnchorDesignController controller,
            bool isAutoScanning,
            bool isVisible)
        {
            _controller = controller;

            TglScan.Value = isAutoScanning;
            TglShow.Value = isVisible;

            UpdateMenuPosition();
        }

        /// <inheritdoc />
        protected override void AfterElementsCreated()
        {
            base.AfterElementsCreated();

            UpdateMenuPosition();

            BtnBack.Activator.OnActivated += _ =>
            {
                if (null != OnBack)
                {
                    OnBack();
                }
            };
            
            BtnReset.Activator.OnActivated += _ =>
            {
                if (null != OnReset)
                {
                    OnReset();
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

        /// <summary>
        /// Updates the position of the menu.
        /// </summary>
        private void UpdateMenuPosition()
        {
            if (null == Root || null == _controller)
            {
                return;
            }

            Root.Schema.Set(
                "position",
                (_controller.transform.position - 0.25f * Camera.main.transform.forward).ToVec());
        }
    }
}