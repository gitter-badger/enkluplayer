using CreateAR.SpirePlayer.IUX;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Controls editing content.
    /// </summary>
    public class EditContentDesignState : IDesignState
    {
        /// <summary>
        /// Design controller.
        /// </summary>
        private DesignController _design;
        
        /// <summary>
        /// Root element for dynamic content.
        /// </summary>
        private Element _dynamicRoot;
        
        /// <summary>
        /// Menu to adjust prop.
        /// </summary>
        private AdjustContentController _adjustContent;

        /// <summary>
        /// Menu to edit prop.
        /// </summary>
        private EditContentController _editContent;

        /// <summary>
        /// Menu to place objects.
        /// </summary>
        private MoveContentController _move;

        public void Initialize(
            DesignController design,
            GameObject unityRoot,
            Element dynamicRoot,
            Element staticRoot)
        {
            _design = design;
            _dynamicRoot = dynamicRoot;

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
                _editContent.OnMove += Edit_OnMove;
                _editContent.OnDelete += Edit_OnDelete;
                _editContent.enabled = false;
            }

            // place content
            {
                _move = unityRoot.AddComponent<MoveContentController>();
                _move.OnConfirmController += Move_OnConfirmController;
                _move.OnCancel += Move_OnCancel;
                _move.enabled = false;
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

        public void CloseAll()
        {
            _adjustContent.enabled = false;
            _editContent.enabled = false;
        }

        /// <summary>
        /// Called when a controller asks to open the menu.
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
        /// Called when the place menu wants to confirm placement.
        /// </summary>
        /// <param name="controller">The prop controller.</param>
        private void Move_OnConfirmController(ContentDesignController controller)
        {
            controller.transform.SetParent(null, true);
            controller.ShowSplashMenu();
            controller.EnableUpdates();

            _move.enabled = false;
            _dynamicRoot.Schema.Set("focus.visible", true);

            // back to main
            _design.ChangeState<MainDesignState>();
        }

        /// <summary>
        /// Called when the place menu wants to cancel placement.
        /// </summary>
        private void Move_OnCancel()
        {
            _move.enabled = false;
            _adjustContent.enabled = true;
            _editContent.enabled = true;
        }

        /// <summary>
        /// Called to move the prop.
        /// </summary>
        /// <param name="elementController">The controller.</param>
        private void Edit_OnMove(ContentDesignController elementController)
        {
            CloseAll();

            elementController.HideSplashMenu();
            elementController.DisableUpdates();

            _move.Initialize(elementController);
            _move.enabled = true;
        }

        /// <summary>
        /// Called to delete the prop.
        /// </summary>
        /// <param name="elementController">The controller.</param>
        private void Edit_OnDelete(ContentDesignController elementController)
        {
            CloseAll();

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
            CloseAll();

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