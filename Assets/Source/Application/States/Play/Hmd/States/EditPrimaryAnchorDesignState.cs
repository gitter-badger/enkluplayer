using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.IUX;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Design state for editing a primary anchor.
    /// </summary>
    public class EditPrimaryAnchorDesignState : IArDesignState
    {
        /// <summary>
        /// Manages UI.
        /// </summary>
        private readonly IUIManager _ui;

        /// <summary>
        /// Capture interface.
        /// </summary>
        private readonly IMeshCaptureService _capture;

        /// <summary>
        /// UI frame.
        /// </summary>
        private UIManagerFrame _frame;

        /// <summary>
        /// Designer for HMD.
        /// </summary>
        private HmdDesignController _designer;

        /// <summary>
        /// Anchor controller.
        /// </summary>
        private AnchorDesignController _controller;

        /// <summary>
        /// Constructor.
        /// </summary>
        public EditPrimaryAnchorDesignState(
            IUIManager ui,
            IMeshCaptureService capture)
        {
            _ui = ui;
            _capture = capture;
        }

        /// <inheritdoc />
        public void Initialize(
            HmdDesignController designer,
            GameObject unityRoot,
            Element dynamicRoot,
            Element staticRoot)
        {
            _designer = designer;
        }

        /// <inheritdoc />
        public void Uninitialize()
        {

        }

        /// <inheritdoc />
        public void Enter(object context)
        {
            _frame = _ui.CreateFrame();

            _controller = (AnchorDesignController) context;

            _ui
                .Open<AdjustPrimaryAnchorUIView>(new UIReference
                {
                    UIDataId = "PrimaryAnchor.Adjust"
                })
                .OnSuccess(el =>
                {
                    el.OnVisibilityChanged += visible => _capture.IsVisible = visible;
                    el.OnAutoScanChanged += autoScan =>
                    {
                        if (autoScan)
                        {
                            _capture.Start();
                        }
                        else
                        {
                            _capture.Stop();
                        }
                    };
                    el.Initialize(_controller);
                })
                .OnFailure(ex =>
                {
                    Log.Error(this, "Could not open PrimaryAnchor.Adjust UI: {0}", ex);

                    _designer.ChangeState<MainDesignState>();
                });
        }

        /// <inheritdoc />
        public void Update(float dt)
        {
            
        }

        /// <inheritdoc />
        public void Exit()
        {
            _frame.Release();
        }
    }
}