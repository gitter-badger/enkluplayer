using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.IUX;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    public class ContentDesignState : IDesignState
    {
        private readonly IElementControllerManager _controllers;

        /// <summary>
        /// Design controller.
        /// </summary>
        private DesignController _design;

        private GameObject _unityRoot;

        private Element _dynamicRoot;

        private Element _staticRoot;

        /// <summary>
        /// New item menu.
        /// </summary>
        private NewContentController _newContent;

        /// <summary>
        /// Menu to place objects.
        /// </summary>
        private PlaceContentController _place;

        /// <summary>
        /// Menu to adjust prop.
        /// </summary>
        private AdjustContentController _adjustContent;

        /// <summary>
        /// Menu to edit prop.
        /// </summary>
        private EditContentController _editContent;

        public ContentDesignState(IElementControllerManager controllers)
        {
            _controllers = controllers;
        }

        public void Initialize(
            DesignController design,
            GameObject unityRoot,
            Element dynamicRoot,
            Element staticRoot)
        {
            _design = design;
            _unityRoot = unityRoot;
            _dynamicRoot = dynamicRoot;
            _staticRoot = staticRoot;

            // new content
            {
                _newContent = unityRoot.AddComponent<NewContentController>();
                _newContent.enabled = false;
                _newContent.OnCancel += New_OnCancel;
                _newContent.OnConfirm += New_OnConfirm;
                dynamicRoot.AddChild(_newContent.Root);
            }

            // place content
            {
                _place = unityRoot.AddComponent<PlaceContentController>();
                _place.OnConfirm += Place_OnConfirm;
                _place.OnConfirmController += Place_OnConfirmController;
                _place.OnCancel += Place_OnCancel;
                _place.enabled = false;
            }

            // adjust content
            {
                _adjustContent = unityRoot.AddComponent<AdjustContentController>();
                _adjustContent.OnExit += PropAdjust_OnExit;
                _adjustContent.enabled = false;
                _adjustContent.SliderRotate.OnVisibilityChanged += PropAdjustControl_OnVisibilityChanged;
                _adjustContent.SliderX.OnVisibilityChanged += PropAdjustControl_OnVisibilityChanged;
                _adjustContent.SliderY.OnVisibilityChanged += PropAdjustControl_OnVisibilityChanged;
                _adjustContent.SliderZ.OnVisibilityChanged += PropAdjustControl_OnVisibilityChanged;
            }

            // edit content
            {
                _editContent = unityRoot.AddComponent<EditContentController>();
                _editContent.OnMove += PropEdit_OnMove;
                _editContent.OnDelete += PropEdit_OnDelete;
                _editContent.enabled = false;
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
            CloseAll();
        }

        /// <summary>
        /// Closes all menus.
        /// </summary>
        private void CloseAll()
        {
            _newContent.enabled = false;
            _place.enabled = false;
            _adjustContent.enabled = false;
            _editContent.enabled = false;
        }

        /// <summary>
        /// Closes prop menus.
        /// </summary>
        private void ClosePropControls()
        {
            _adjustContent.enabled = false;
            _editContent.enabled = false;
        }

        /// <summary>
        /// Called when the new menu wants to create an element.
        /// </summary>
        private void New_OnConfirm(string assetId)
        {
            _newContent.enabled = false;

            _place.Initialize(assetId);
            _place.enabled = true;
        }

        /// <summary>
        /// Called when the new menu wants to cancel.
        /// </summary>
        private void New_OnCancel()
        {
            _newContent.enabled = false;
            _dynamicRoot.Schema.Set("focus.visible", true);
            
            // back to main
            _design.ChangeState<MainDesignState>();
        }

        /// <summary>
        /// Called when the place menu wants to confirm placement.
        /// </summary>
        /// <param name="contentData">The prop.</param>
        private void Place_OnConfirm(ElementData contentData)
        {
            _design
                .Active
                .Create(contentData)
                //.OnSuccess(controller => controller.OnAdjust += Controller_OnAdjust)
                .OnFailure(exception =>
                {
                    Log.Error(this, "Could not place content : {0}.", exception);
                });

            _place.enabled = false;
            _dynamicRoot.Schema.Set("focus.visible", true);

            // back to main
            _design.ChangeState<MainDesignState>();
        }

        /// <summary>
        /// Called when the place menu wants to confirm placement.
        /// </summary>
        /// <param name="controller">The prop controller.</param>
        private void Place_OnConfirmController(ContentDesignController controller)
        {
            controller.transform.SetParent(null, true);
            controller.ShowSplashMenu();
            controller.EnableUpdates();

            _place.enabled = false;
            _dynamicRoot.Schema.Set("focus.visible", true);

            // back to main
            _design.ChangeState<MainDesignState>();
        }

        /// <summary>
        /// Called when the place menu wants to cancel placement.
        /// </summary>
        private void Place_OnCancel()
        {
            _place.enabled = false;

            _newContent.enabled = true;
        }

        /// <summary>
        /// Called when the controller asks to open the menu.
        /// </summary>
        /// <param name="controller">The prop controller.</param>
        private void Controller_OnAdjust(ContentDesignController controller)
        {
            // hide the splash on the controller
            controller.HideSplashMenu();
            
            _dynamicRoot.Schema.Set("focus.visible", false);

            _adjustContent.Initialize(controller);
            _adjustContent.enabled = true;

            _editContent.Initialize(controller);
            _editContent.enabled = true;
        }

        /// <summary>
        /// Called to move the prop.
        /// </summary>
        /// <param name="elementController">The controller.</param>
        private void PropEdit_OnMove(ContentDesignController elementController)
        {
            ClosePropControls();

            elementController.HideSplashMenu();
            elementController.DisableUpdates();

            _place.Initialize(elementController);
            _place.enabled = true;
        }

        /// <summary>
        /// Called to delete the prop.
        /// </summary>
        /// <param name="elementController">The controller.</param>
        private void PropEdit_OnDelete(ContentDesignController elementController)
        {
            ClosePropControls();

            _design.Active.Destroy(elementController.Element.Id);

            _dynamicRoot.Schema.Set("focus.visible", true);

            // back to main
            _design.ChangeState<MainDesignState>();
        }

        /// <summary>
        /// Called when the prop adjust wishes to exit.
        /// </summary>
        private void PropAdjust_OnExit(ContentDesignController controller)
        {
            ClosePropControls();

            controller.ShowSplashMenu();

            _dynamicRoot.Schema.Set("focus.visible", true);

            // back to main
            _design.ChangeState<MainDesignState>();
        }

        /// <summary>
        /// Called when prop adjust control visibility is changed.
        /// </summary>
        /// <param name="interactable">Interactable.</param>
        private void PropAdjustControl_OnVisibilityChanged(IInteractable interactable)
        {
            _editContent.enabled = !interactable.Visible;
        }
    }
}