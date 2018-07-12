using System;
using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.IUX;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Controls editing content.
    /// </summary>
    public class EditElementDesignState : IArDesignState
    {
        /// <summary>
        /// Handles voice commands.
        /// </summary>
        private readonly IVoiceCommandManager _voice;

        /// <summary>
        /// Controls UI.
        /// </summary>
        private readonly IUIManager _ui;

        /// <summary>
        /// Design controller.
        /// </summary>
        private HmdDesignController _design;
        
        /// <summary>
        /// Root element for dynamic content.
        /// </summary>
        private Element _dynamicRoot;
        
        /// <summary>
        /// Menu to adjust prop.
        /// </summary>
        private AdjustElementController _adjustElement;

        /// <summary>
        /// Menu to edit prop.
        /// </summary>
        private EditElementController _editElement;

        /// <summary>
        /// Menu to place objects.
        /// </summary>
        private MoveElementController _move;

        /// <summary>
        /// Content controller.
        /// </summary>
        private ElementSplashDesignController _controller;

        /// <summary>
        /// UI frame.
        /// </summary>
        private UIManagerFrame _frame;

        /// <summary>
        /// Constructor.
        /// </summary>
        public EditElementDesignState(
            IUIManager ui,
            IVoiceCommandManager voice)
        {
            _ui = ui;
            _voice = voice;
        }

        /// <inheritdoc />
        public void Initialize(
            HmdDesignController design,
            GameObject unityRoot,
            Element dynamicRoot,
            Element staticRoot)
        {
            _design = design;
            _dynamicRoot = dynamicRoot;

            // adjust content
            {
                _adjustElement = unityRoot.AddComponent<AdjustElementController>();
                _adjustElement.OnExit += PropAdjust_OnExit;
                _adjustElement.enabled = false;
                _adjustElement.SliderRotate.OnVisibilityChanged += AdjustControl_OnVisibilityChanged;
                _adjustElement.SliderX.OnVisibilityChanged += AdjustControl_OnVisibilityChanged;
                _adjustElement.SliderY.OnVisibilityChanged += AdjustControl_OnVisibilityChanged;
                _adjustElement.SliderZ.OnVisibilityChanged += AdjustControl_OnVisibilityChanged;
            }

            // edit content
            {
                _editElement = unityRoot.AddComponent<EditElementController>();
                _editElement.OnMove += Edit_OnMove;
                _editElement.OnReparent += Edit_OnReparent;
                _editElement.OnDuplicate += Edit_OnDuplicate;
                _editElement.OnDelete += Edit_OnDelete;
                _editElement.enabled = false;
            }

            // place content
            {
                _move = unityRoot.AddComponent<MoveElementController>();
                _move.OnConfirm += Move_OnConfirmController;
                _move.OnCancel += Move_OnCancel;
                _move.enabled = false;
            }
        }

        /// <inheritdoc />
        public void Uninitialize()
        {
            Object.Destroy(_adjustElement);
            Object.Destroy(_editElement);
            Object.Destroy(_move);
        }

        /// <inheritdoc />
        public void Enter(object context)
        {
            Log.Info(this, "Entering {0}.", GetType().Name);

            _controller = (ElementSplashDesignController) context;

            // hide the splash on the controller
            _controller.HideSplashMenu();

            _dynamicRoot.Schema.Set("focus.visible", false);

            _adjustElement.Initialize(_controller);
            _adjustElement.enabled = true;

            _editElement.Initialize(_controller);
            _editElement.enabled = true;

            _voice.Register("back", Voice_OnBack);

            _frame = _ui.CreateFrame();
        }

        /// <inheritdoc />
        public void Update(float dt)
        {
            
        }

        /// <inheritdoc />
        public void Exit()
        {
            CloseAll();
            _frame.Release();

            _voice.Unregister("back");

            Log.Info(this, "Exiting {0}.", GetType().Name);
        }

        /// <summary>
        /// Closes all dialogs.
        /// </summary>
        private void CloseAll()
        {
            _adjustElement.enabled = false;
            _editElement.enabled = false;
        }
        
        /// <summary>
        /// Called when the place menu wants to confirm placement.
        /// </summary>
        /// <param name="controller">The prop controller.</param>
        private void Move_OnConfirmController(ElementSplashDesignController controller)
        {
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
            _adjustElement.enabled = true;
            _editElement.enabled = true;
        }

        /// <summary>
        /// Called to move the content.
        /// </summary>
        /// <param name="elementController">The controller.</param>
        private void Edit_OnMove(ElementSplashDesignController elementController)
        {
            CloseAll();
            
            elementController.EnableUpdates();
            _move.Initialize(elementController);
            _move.enabled = true;
        }

        /// <summary>
        /// Called to reparent.
        /// </summary>
        /// <param name="controller">The controller.</param>
        private void Edit_OnReparent(ElementSplashDesignController controller)
        {
            CloseAll();

            _design.ChangeState<ReparentDesignState>(controller);
        }

        /// <summary>
        /// Called when the edit menu asks to duplicate an element.
        /// </summary>
        /// <param name="controller"></param>
        private void Edit_OnDuplicate(ElementSplashDesignController controller)
        {
            CloseAll();

            // duplicate data
            var element = controller.Element;
            var data = new ElementData(element);
            GenerateSafeIds(data);

            // find parent
            var parent = controller.Element.Parent;

            _design
                .Elements
                .Create(data, null == parent ? "root" : parent.Id)
                .OnSuccess(el =>
                {
                    // back to main
                    _design.ChangeState<MainDesignState>();
                })
                .OnFailure(exception =>
                {
                    Log.Info(this, "Could not duplicate element : {0}.", exception);

                    _ui
                        .Open<ICommonErrorView>(new UIReference
                        {
                            UIDataId = UIDataIds.ERROR
                        })
                        .OnSuccess(el =>
                        {
                            el.Message = "There was an error duplicating this element. Try again later.";
                            el.Action = "Ok";
                            el.OnOk += () => _ui.Pop();
                        });
                });
        }

        /// <summary>
        /// Called to delete the prop.
        /// </summary>
        /// <param name="elementController">The controller.</param>
        private void Edit_OnDelete(ElementSplashDesignController elementController)
        {
            CloseAll();

            _design.Elements.Destroy(elementController.Element);

            _dynamicRoot.Schema.Set("focus.visible", true);

            // back to main
            _design.ChangeState<MainDesignState>();
        }

        /// <summary>
        /// Called when the prop adjust wishes to exit.
        /// </summary>
        private void PropAdjust_OnExit(ElementSplashDesignController controller)
        {
            // force final state update
            controller.FinalizeState();

            // close splash menu
            controller.ShowSplashMenu();

            // reenable dynamic
            _dynamicRoot.Schema.Set("focus.visible", true);

            // back to main
            _design.ChangeState<MainDesignState>();
        }

        /// <summary>
        /// Called when adjust control visibility is changed.
        /// </summary>
        /// <param name="interactable">Interactable.</param>
        private void AdjustControl_OnVisibilityChanged(IInteractable interactable)
        {
            var visible = interactable.Visible;

            _editElement.enabled = !visible;

            if (visible)
            {
                _controller.EnableUpdates();
            }
            else
            {
                _controller.DisableUpdates();
            }
        }

        /// <summary>
        /// Called when the voice manager receives a back command.
        /// </summary>
        /// <param name="command">The command.</param>
        private void Voice_OnBack(string command)
        {
            // back to main
            _design.ChangeState<MainDesignState>();
        }

        /// <summary>
        /// Generates new Ids for duplication/
        /// </summary>
        /// <param name="data">The element data.</param>
        /// <returns></returns>
        private static void GenerateSafeIds(ElementData data)
        {
            var id = Guid.NewGuid().ToString();

            data.Id = id;
            data.Schema.Strings["id"] = id;

            foreach (var child in data.Children)
            {
                GenerateSafeIds(child);
            }
        }
    }
}