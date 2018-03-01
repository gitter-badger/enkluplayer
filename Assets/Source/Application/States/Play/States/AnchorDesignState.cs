using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.IUX;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    public class AnchorDesignState : IDesignState
    {
        private readonly IElementControllerManager _controllers;
        private readonly IHttpService _http;
        private readonly IWorldAnchorProvider _provider;

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

        /// <summary>
        /// Distance filter.
        /// </summary>
        private readonly DistanceElementControllerFilter _distanceFilter = new DistanceElementControllerFilter();

        /// <summary>
        /// Content filter.
        /// </summary>
        private readonly TypeElementControllerFilter _anchorFilter = new TypeElementControllerFilter(typeof(WorldAnchorWidget));

        public AnchorDesignState(
            IElementControllerManager controllers,
            IHttpService http,
            IWorldAnchorProvider provider)
        {
            _controllers = controllers;
            _http = http;
            _provider = provider;
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
            _controllers
                .Filter(_distanceFilter)
                .Filter(_anchorFilter)
                .Add<AnchorDesignController>(new AnchorDesignController.AnchorDesignControllerContext
                {
                    Config = _design.Config,
                    Http = _http,
                    Provider = _provider
                });
        }

        public void Update(float dt)
        {
            
        }

        public void Exit()
        {
            _controllers
                .Remove<AnchorDesignController>()
                .Unfilter(_distanceFilter)
                .Unfilter(_anchorFilter);

            CloseAll();
        }

        private void CloseAll()
        {
            _anchors.enabled = false;
            _placeAnchor.enabled = false;
        }

        /// <summary>
        /// Called when show children option has changed on anchors.
        /// </summary>
        /// <param name="value">The value.</param>
        private void Anchors_OnShowChildrenChanged(bool value)
        {
            //_design.Active.ShowAnchorChildren = value;
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

            _design
                .Active
                .Create(data)
                .OnSuccess(element => _design.ChangeState<MainDesignState>())
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