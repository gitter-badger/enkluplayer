using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.IUX;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    public class AnchorDesignState : IDesignState
    {
        private readonly IAdminAppController _app;

        /// <summary>
        /// Design controller.
        /// </summary>
        private DesignController _design;

        /// <summary>
        /// Anchor menu.
        /// </summary>
        private AnchorMenuController _anchors;

        /// <summary>
        /// Menu for placing anchors.
        /// </summary>
        private PlaceAnchorController _placeAnchor;

        public AnchorDesignState(IAdminAppController app)
        {
            _app = app;
        }

        public void Initialize(
            DesignController design,
            GameObject unityRoot,
            Element dynamicRoot,
            Element staticRoot)
        {
            _design = design;

            // main anchor menu
            {
                _anchors = unityRoot.AddComponent<AnchorMenuController>();
                _anchors.enabled = false;
                _anchors.OnBack += Anchors_OnBack;
                _anchors.OnNew += Anchors_OnNew;
                _anchors.OnShowChildrenChanged += Anchors_OnShowChildrenChanged;
                dynamicRoot.AddChild(_anchors.Root);
            }

            // place anchor menu
            {
                _placeAnchor = unityRoot.AddComponent<PlaceAnchorController>();
                _placeAnchor.OnCancel += PlaceAnchor_OnCancel;
                _placeAnchor.OnOk += PlaceAnchor_OnOk;
                _placeAnchor.enabled = false;
            }
        }

        public void Enter(object context)
        {
            
        }

        public void Update(float dt)
        {
            
        }

        public void Exit()
        {
            
        }

        /// <summary>
        /// Called when show children option has changed on anchors.
        /// </summary>
        /// <param name="value">The value.</param>
        private void Anchors_OnShowChildrenChanged(bool value)
        {
            _app.Active.ShowAnchorChildren = value;
        }

        /// <summary>
        /// Called when new anchor is requested.
        /// </summary>
        private void Anchors_OnNew()
        {
            _anchors.enabled = false;
            _placeAnchor.Initialize();
            _placeAnchor.enabled = true;
        }

        /// <summary>
        /// Called when back button is pressed on anchor menu.
        /// </summary>
        private void Anchors_OnBack()
        {
            _anchors.enabled = false;
            
            // back to main
            _design.ChangeState<MainDesignState>();
        }

        /// <summary>
        /// Called when place anchor confirms placement.
        /// </summary>
        private void PlaceAnchor_OnOk(ElementData data)
        {
            _placeAnchor.enabled = false;
            _anchors.enabled = true;

            _app
                .Active
                .CreateAnchor(data)
                .OnSuccess(controller =>
                {
                    _design.ChangeState<MainDesignState>();
                })
                .OnFailure(exception => Log.Error(this,
                    "Could not create anchor : {0}.",
                    exception));
        }

        /// <summary>
        /// Called when place anchor cancels placement.
        /// </summary>
        private void PlaceAnchor_OnCancel()
        {
            _placeAnchor.enabled = false;
        }
    }
}