using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.IUX;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    public class EditPrimaryAnchorDesignState : IArDesignState
    {
        private readonly IUIManager _ui;

        private UIManagerFrame _frame;
        private HmdDesignController _designer;
        private AnchorDesignController _controller;

        public EditPrimaryAnchorDesignState(
            IUIManager ui)
        {
            _ui = ui;
        }

        public void Initialize(
            HmdDesignController designer,
            GameObject unityRoot,
            Element dynamicRoot,
            Element staticRoot)
        {
            _designer = designer;
        }

        public void Uninitialize()
        {

        }

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

                })
                .OnFailure(ex =>
                {
                    Log.Error(this, "Could not open PrimaryAnchor.Adjust UI: {0}", ex);

                    _designer.ChangeState<MainDesignState>();
                });
        }

        public void Update(float dt)
        {
            
        }

        public void Exit()
        {
            _frame.Release();
        }
    }
}