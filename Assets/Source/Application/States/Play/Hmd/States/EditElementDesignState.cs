using System;
using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.IUX;
using UnityEngine;

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
        /// Content controller.
        /// </summary>
        private ElementSplashDesignController _controller;

        /// <summary>
        /// UI frame.
        /// </summary>
        private UIManagerFrame _frame;

        /// <summary>
        /// Id of adjust menu.
        /// </summary>
        private int _adjustId;

        /// <summary>
        /// Id of edit menu.
        /// </summary>
        private int _editId;

        /// <summary>
        /// Id of move menu.
        /// </summary>
        private int _moveId;

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
        }

        /// <inheritdoc />
        public void Uninitialize()
        {
            //
        }

        /// <inheritdoc />
        public void Enter(object context)
        {
            Log.Info(this, "Entering {0}.", GetType().Name);

            _controller = (ElementSplashDesignController) context;
            _frame = _ui.CreateFrame();

            _voice.Register("back", Voice_OnBack);

            OpenAdjustMenu();
            OpenEditMenu();
        }

        private void OpenAdjustMenu()
        {
            _ui
                .Open<AdjustElementUIView>(new UIReference
                {
                    UIDataId = "Element.Adjust"
                }, out _adjustId)
                .OnSuccess(el =>
                {
                    el.OnExit += PropAdjust_OnExit;
                    el.SliderRotate.OnVisibilityChanged += AdjustControl_OnVisibilityChanged;
                    el.SliderX.OnVisibilityChanged += AdjustControl_OnVisibilityChanged;
                    el.SliderY.OnVisibilityChanged += AdjustControl_OnVisibilityChanged;
                    el.SliderZ.OnVisibilityChanged += AdjustControl_OnVisibilityChanged;
                    el.Initialize(_controller);
                })
                .OnFailure(exception =>
                {
                    Log.Error(this, "Could not open AdjustElementUIView : {0}", exception);

                    _design.ChangeState<MainDesignState>();
                });
        }

        private void OpenEditMenu()
        {
            _ui
                .Open<EditElementUIView>(new UIReference
                {
                    UIDataId = "Element.Edit"
                }, out _editId)
                .OnSuccess(el =>
                {
                    el.OnMove += Edit_OnMove;
                    el.OnReparent += Edit_OnReparent;
                    el.OnDuplicate += Edit_OnDuplicate;
                    el.OnDelete += Edit_OnDelete;
                    el.Initialize(_controller);
                })
                .OnFailure(exception =>
                {
                    Log.Error(this, "Could not open EditElementUIView : {0}", exception);

                    _design.ChangeState<MainDesignState>();
                });
        }

        private void OpenMoveMenu()
        {
            _ui
                .Open<MoveElementUIView>(new UIReference
                {
                    UIDataId = "Element.Move"
                }, out _moveId)
                .OnSuccess(el =>
                {
                    el.OnConfirm += Move_OnConfirmController;
                    el.OnCancel += Move_OnCancel;
                    el.Initialize(_controller);
                })
                .OnFailure(exception =>
                {
                    Log.Error(this, "Could not open MoveElementUIView : {0}", exception);

                    _design.ChangeState<MainDesignState>();
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

            _voice.Unregister("back");

            Log.Info(this, "Exiting {0}.", GetType().Name);
        }

        /// <summary>
        /// Called when the place menu wants to confirm placement.
        /// </summary>
        /// <param name="controller">The prop controller.</param>
        private void Move_OnConfirmController(ElementSplashDesignController controller)
        {
            // back to main
            _design.ChangeState<MainDesignState>();
        }

        /// <summary>
        /// Called when the place menu wants to cancel placement.
        /// </summary>
        private void Move_OnCancel()
        {
            _ui.Close(_moveId);

            OpenAdjustMenu();
            OpenEditMenu();
        }

        /// <summary>
        /// Called to move the content.
        /// </summary>
        /// <param name="elementController">The controller.</param>
        private void Edit_OnMove(ElementSplashDesignController elementController)
        {
            elementController.EnableUpdates();

            _ui.Close(_adjustId);
            _ui.Close(_editId);
            OpenMoveMenu();
        }

        /// <summary>
        /// Called to reparent.
        /// </summary>
        /// <param name="controller">The controller.</param>
        private void Edit_OnReparent(ElementSplashDesignController controller)
        {
            _design.ChangeState<ReparentDesignState>(controller);
        }

        /// <summary>
        /// Called when the edit menu asks to duplicate an element.
        /// </summary>
        /// <param name="controller"></param>
        private void Edit_OnDuplicate(ElementSplashDesignController controller)
        {
            // close other UIs
            _ui.Close(_adjustId);
            _ui.Close(_editId);

            // TODO: Show progress indicator.

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
            // close UIs
            _ui.Close(_adjustId);
            _ui.Close(_editId);
            
            int id;
            _ui
                .Open<ConfirmationUIView>(new UIReference
                {
                    UIDataId = "Common.Confirmation"
                }, out id)
                .OnSuccess(el =>
                {
                    el.Message = "Are you sure you want to DELETE this element?";
                    el.OnCancel += () =>
                    {
                        _ui.Close(id);

                        _design.ChangeState<MainDesignState>();
                    };
                    el.OnConfirm += () =>
                    {
                        _design.Elements.Destroy(elementController.Element);
                        
                        // back to main
                        _design.ChangeState<MainDesignState>();
                    };
                })
                .OnFailure(exception =>
                {
                    Log.Error(this, "Could not open confirmation dialog : {0}", exception);

                    _design.ChangeState<MainDesignState>();
                });
        }

        /// <summary>
        /// Called when the prop adjust wishes to exit.
        /// </summary>
        private void PropAdjust_OnExit(ElementSplashDesignController controller)
        {
            // force final state update
            controller.FinalizeState();
            
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

            //_editElement.enabled = !visible;

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