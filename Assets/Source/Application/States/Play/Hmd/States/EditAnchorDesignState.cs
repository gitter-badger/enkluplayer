using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.IUX;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Design state for editing anchors.
    /// </summary>
    public class EditAnchorDesignState : IArDesignState
    {
        /// <summary>
        /// Makes Http requests.
        /// </summary>
        private readonly IHttpService _http;

        /// <summary>
        /// Caches world anchor data.
        /// </summary>
        private readonly IWorldAnchorCache _cache;

        /// <summary>
        /// Provides anchor import/export.
        /// </summary>
        private readonly IWorldAnchorProvider _provider;

        /// <summary>
        /// Updates elements.
        /// </summary>
        private readonly IElementUpdateDelegate _elementUpdater;

        /// <summary>
        /// UI management.
        /// </summary>
        private readonly IUIManager _ui;

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

        private AnchorDesignController _controller;
        private int _adjustId;
        private int _moveId;

        /// <summary>
        /// Constructor.
        /// </summary>
        public EditAnchorDesignState(
            IHttpService http,
            IWorldAnchorCache cache,
            IWorldAnchorProvider provider,
            IElementUpdateDelegate elementUpdater,
            IUIManager ui)
        {
            _http = http;
            _cache = cache;
            _provider = provider;
            _elementUpdater = elementUpdater;
            _ui = ui;
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
        /// Re-exports an anchor and updates associated data.
        /// </summary>
        private void ResaveAnchor(AnchorDesignController controller)
        {
            var anchor = controller.Anchor;

            // increment version
            var version = anchor.Schema.Get<int>("version").Value + 1;

            // renderer should show saving!
            controller.Renderer.Poll = AnchorRenderer.PollType.Forced;
            controller.Renderer.ForcedColor = Color.cyan;

            // export
            Log.Info(this, "Re-save anchor called. Beginning export and re-upload process.");

            _provider
                .Export(anchor.Id, anchor.GameObject)
                .OnSuccess(bytes =>
                {
                    Log.Info(this, "Successfully exported. Progressing to upload.");

                    // save to cache
                    _cache.Save(anchor.Id, version, bytes);

                    controller.Renderer.ForcedColor = Color.gray;

                    ReuploadAnchor(controller, bytes, anchor, version, 3);
                })
                .OnFailure(exception =>
                {
                    Log.Error(this,
                        "Could not export anchor : {0}.",
                        exception);

                    controller.Renderer.ForcedColor = Color.red;
                });
        }
        
        /// <summary>
        /// Updates an anchor.
        /// </summary>
        private void ReuploadAnchor(
            AnchorDesignController controller,
            byte[] bytes,
            WorldAnchorWidget anchor,
            int version,
            int retries)
        {
            var url = string.Format(
                "/editor/app/{0}/scene/{1}/anchor/{2}",
                _design.App.Id,
                _elementUpdater.Active,
                anchor.Id);

            _http
                .PutFile<Trellis.Messages.UploadAnchor.Response>(
                    _http.Urls.Url(url),
                    new Commons.Unity.DataStructures.Tuple<string, string>[0],
                    ref bytes)
                .OnSuccess(response =>
                {
                    if (response.Payload.Success)
                    {
                        Log.Info(this, "Successfully reuploaded anchor.");

                        // complete, now send out network update
                        _elementUpdater.Update(anchor, "src", url);
                        _elementUpdater.Update(anchor, "version", version);
                        _elementUpdater.Update(anchor, "position", anchor.Schema.Get<Vec3>("position").Value);
                        _elementUpdater.FinalizeUpdate(anchor);

                        // show controller as ready again
                        controller.Renderer.Poll = AnchorRenderer.PollType.Dynamic;
                    }
                    else
                    {
                        Log.Error(this,
                            "Anchor reupload error : {0}.",
                            response.Payload.Error);

                        controller.Renderer.ForcedColor = Color.red;
                    }
                })
                .OnFailure(exception =>
                {
                    Log.Error(this,
                        "Could not reupload anchor : {0}.",
                        exception);

                    if (--retries > 0)
                    {
                        Log.Info(this, "Retry reuploading anchor.");

                        ReuploadAnchor(controller, bytes, anchor, version, retries);
                    }
                    else
                    {
                        controller.Renderer.ForcedColor = Color.red;
                    }
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
            
            ResaveAnchor(_moveController);

            _design.ChangeState<MainDesignState>();
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
            ResaveAnchor(anchorDesignController);

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