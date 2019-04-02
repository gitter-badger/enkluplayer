using CreateAR.Commons.Unity.Logging;
using CreateAR.EnkluPlayer.IUX;
using Enklu.Data;
using UnityEngine;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Design state for editing anchors.
    /// </summary>
    public class EditAnchorDesignState : IArDesignState
    {
        /// <summary>
        /// Updates elements.
        /// </summary>
        private readonly IElementUpdateDelegate _elementUpdater;

        /// <summary>
        /// UI management.
        /// </summary>
        private readonly IUIManager _ui;

        /// <summary>
        /// Txns.
        /// </summary>
        private readonly IElementTxnManager _txns;

        /// <summary>
        /// UI frame.
        /// </summary>
        private UIManagerFrame _frame;

        /// <summary>
        /// Design controller.
        /// </summary>
        private HmdDesignController _design;

        /// <summary>
        /// Controller we're moving.
        /// </summary>
        private AnchorDesignController _moveController;

        /// <summary>
        /// Controller passed in.
        /// </summary>
        private AnchorDesignController _controller;

        /// <summary>
        /// Id of the adjust menu.
        /// </summary>
        private int _adjustId;

        /// <summary>
        /// Id of the move menu.
        /// </summary>
        private int _moveId;

        /// <summary>
        /// Constructor.
        /// </summary>
        public EditAnchorDesignState(
            IElementUpdateDelegate elementUpdater,
            IUIManager ui,
            IElementTxnManager txns)
        {
            _elementUpdater = elementUpdater;
            _ui = ui;
            _txns = txns;
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
            Log.Info(this, "Entering {0}", GetType().Name);

            _controller = (AnchorDesignController) context;

            _frame = _ui.CreateFrame();

            OpenAdjustMenu();
        }

        /// <inheritdoc />
        public void Update(float dt)
        {

        }

        /// <inheritdoc />
        public void Exit()
        {
            _frame.Release();

            Log.Info(this, "Exited {0}", GetType().Name);
        }

        /// <summary>
        /// Opens the adjust menu.
        /// </summary>
        private void OpenAdjustMenu()
        {
            _ui
                .Open<AdjustAnchorUIView>(new UIReference
                {
                    UIDataId = "Anchor.Adjust"
                }, out _adjustId)
                .OnSuccess(el =>
                {
                    el.OnDelete += Adjust_OnDelete;
                    el.OnMove += Adjust_OnMove;
                    el.OnReload += Adjust_OnReload;
                    el.OnResave += Adjust_OnResave;
                    el.OnExit += Adjust_OnExit;
                    el.Initialize(_controller);
                })
                .OnFailure(exception =>
                {
                    Log.Error(this, "Could not open Anchor.Adjust menu : {0}", exception);

                    _design.ChangeState<MainDesignState>();
                });
        }

        /// <summary>
        /// Opens the move menu.
        /// </summary>
        private void OpenMoveMenu()
        {
            _ui
                .Open<PlaceAnchorUIView>(new UIReference
                {
                    UIDataId = "Anchor.Place"
                }, out _moveId)
                .OnSuccess(el =>
                {
                    el.OnCancel += MoveAnchor_OnCancel;
                    el.OnOk += MoveAnchor_OnOk;
                    el.Initialize();
                })
                .OnFailure(exception =>
                {
                    Log.Error(this, "Could not open Anchor.Place menu : {0}", exception);

                    _design.ChangeState<MainDesignState>();
                });
        }

        /// <summary>
        /// Called when the move menu wants to finish moving.
        /// </summary>
        /// <param name="data">Data for the element.</param>
        private void MoveAnchor_OnOk(ElementData data)
        {
            _ui.Close(_moveId);

            // move into position and reexport
            var position = data.Schema.Vectors["position"];
            _moveController.Anchor.Schema.Set("position", position);
            _moveController.Anchor.GameObject.transform.position = position.ToVector();
            _moveController.Renderer.gameObject.SetActive(true);

            _txns
                .Request(new ElementTxn(_elementUpdater.Active).Update(_moveController.Anchor.Id, "position.rel", position))
                .OnSuccess(_ =>
                {
                    _moveController.Anchor.Export(
                        _design.App.Id,
                        _elementUpdater.Active);

                    _design.ChangeState<MainDesignState>();
                })
                .OnFailure(ex =>
                {
                    Log.Error(this, "Could not update relative positions!");

                    _design.ChangeState<MainDesignState>();
                });
        }

        /// <summary>
        /// Called when the move menu wants to cancel movement.
        /// </summary>
        private void MoveAnchor_OnCancel()
        {
            _ui.Close(_moveId);
            OpenAdjustMenu();

            // re-enable
            _moveController.Renderer.gameObject.SetActive(true);
            _moveController.Anchor.Reload();
        }

        /// <summary>
        /// Called when adjust menu wants to exit.
        /// </summary>
        private void Adjust_OnExit(AnchorDesignController controller)
        {
            _design.ChangeState<MainDesignState>();
        }

        /// <summary>
        /// Called when the adjust menu wants to delete an element.
        /// </summary>
        private void Adjust_OnDelete(AnchorDesignController controller)
        {
            _ui.Close(_adjustId);

            int id;
            _ui
                .Open<ConfirmationUIView>(new UIReference
                {
                    UIDataId = "Common.Confirmation"
                }, out id)
                .OnSuccess(el =>
                {
                    el.Message = "Are you sure you want to DELETE this anchor?";
                    el.OnCancel += () =>
                    {
                        _ui.Close(id);

                        OpenAdjustMenu();
                    };
                    el.OnConfirm += () =>
                    {
                        _ui.Close(id);

                        // TODO: Show progress indicator.

                        _elementUpdater
                            .Destroy(controller.Element)
                            .OnFinally(_ => _design.ChangeState<MainDesignState>())
                            .OnFailure(exception =>
                            {
                                Log.Error(this,
                                    "Could not delete element : {0}.",
                                    exception);
                            });
                    };
                })
                .OnFailure(exception =>
                {
                    Log.Error(this, "Could not open confirmation dialog : {0}", exception);
                    OpenAdjustMenu();

                });
        }

        /// <summary>
        /// Resaves an anchor.
        /// </summary>
        /// <param name="anchorDesignController">The controller.</param>
        private void Adjust_OnResave(AnchorDesignController anchorDesignController)
        {
            anchorDesignController.Anchor.Export(
                _design.App.Id,
                _elementUpdater.Active);

            _design.ChangeState<MainDesignState>();
        }

        /// <summary>
        /// Reloads an anchor.
        /// </summary>
        /// <param name="anchorDesignController">The controller.</param>
        private void Adjust_OnReload(AnchorDesignController anchorDesignController)
        {
            anchorDesignController.Anchor.Reload();

            _design.ChangeState<MainDesignState>();
        }

        /// <summary>
        /// Moves an anchor-- essentially replacing the world anchor export data.
        /// </summary>
        /// <param name="anchorDesignController">The controller.</param>
        private void Adjust_OnMove(AnchorDesignController anchorDesignController)
        {
            _ui.Close(_adjustId);

            // unlock + hide
            _moveController = anchorDesignController;
            _moveController.Anchor.Schema.Set("locked", false);
            _moveController.Renderer.gameObject.SetActive(false);

            // open move menu
            OpenMoveMenu();
        }
    }
}