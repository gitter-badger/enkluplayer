using System.Diagnostics;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.IUX;
using UnityEngine;
using Object = UnityEngine.Object;

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
        /// Design controller.
        /// </summary>
        private HmdDesignController _design;
        
        /// <summary>
        /// Menu for moving anchors.
        /// </summary>
        private PlaceAnchorUIView _moveAnchor;

        /// <summary>
        /// Adjusts anchors.
        /// </summary>
        private AdjustAnchorController _adjustAnchor;

        /// <summary>
        /// Controller we're moving.
        /// </summary>
        private AnchorDesignController _moveController;

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
            
            // move anchor menu
            {
                _moveAnchor = unityRoot.AddComponent<PlaceAnchorUIView>();
                _moveAnchor.OnCancel += MoveAnchor_OnCancel;
                _moveAnchor.OnOk += MoveAnchor_OnOk;
                _moveAnchor.enabled = false;
            }
            
            // adjust menu
            {
                _adjustAnchor = unityRoot.AddComponent<AdjustAnchorController>();
                _adjustAnchor.OnDelete += Adjust_OnDelete;
                _adjustAnchor.OnMove += Adjust_OnMove;
                _adjustAnchor.OnReload += Adjust_OnReload;
                _adjustAnchor.OnResave += Adjust_OnResave;
                _adjustAnchor.OnExit += Adjust_OnExit;
                _adjustAnchor.enabled = false;
            }
        }

        /// <inheritdoc />
        public void Uninitialize()
        {
            Object.Destroy(_adjustAnchor);
            Object.Destroy(_moveAnchor);
        }

        /// <inheritdoc />
        public void Enter(object context)
        {
            Log.Info(this, "Entering {0}", GetType().Name);

            var controller = (AnchorDesignController) context;
        }

        /// <inheritdoc />
        public void Update(float dt)
        {
            
        }

        /// <inheritdoc />
        public void Exit()
        {
            CloseAll();

            Log.Info(this, "Exited {0}", GetType().Name);
        }

        /// <summary>
        /// Closes all menus.
        /// </summary>
        private void CloseAll()
        {
            _adjustAnchor.enabled = false;
            _moveAnchor.enabled = false;
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
            controller.Renderer.PlaceholderSaving();

            // export
            Log.Info(this, "Re-save anchor called. Beginning export and re-upload process.");

            _provider
                .Export(anchor.Id, anchor.GameObject)
                .OnSuccess(bytes =>
                {
                    Log.Info(this, "Successfully exported. Progressing to upload.");

                    // save to cache
                    _cache.Save(anchor.Id, version, bytes);

                    ReuploadAnchor(controller, bytes, anchor, version, 3);
                })
                .OnFailure(exception =>
                {
                    Log.Error(this,
                        "Could not export anchor : {0}.",
                        exception);

                    controller.Renderer.PlaceholderError();
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
                        //controller.Renderer.Ready();
                    }
                    else
                    {
                        Log.Error(this,
                            "Anchor reupload error : {0}.",
                            response.Payload.Error);

                        //controller.Renderer.Error();
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
                        //controller.Renderer.Error();
                    }
                });
        }

        /// <summary>
        /// Called when the move menu wants to finish moving.
        /// </summary>
        /// <param name="data">Data for the element.</param>
        private void MoveAnchor_OnOk(ElementData data)
        {
            _moveAnchor.enabled = false;

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
            _moveAnchor.enabled = false;
            _adjustAnchor.enabled = true;

            // re-enable
            _moveController.Renderer.gameObject.SetActive(true);
            _moveController.Anchor.Reload();
        }

        /// <summary>
        /// Called when adjust menu wants to exit.
        /// </summary>
        private void Adjust_OnExit(AnchorDesignController controller)
        {
            CloseAll();

            _design.ChangeState<MainDesignState>();
        }

        /// <summary>
        /// Called when the adjust menu wants to delete an element.
        /// </summary>
        private void Adjust_OnDelete(AnchorDesignController controller)
        {
            _adjustAnchor.enabled = false;

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
                        _adjustAnchor.enabled = true;

                        _ui.Close(id);
                    };
                    el.OnConfirm += () =>
                    {
                        _ui.Close(id);

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

                    _adjustAnchor.enabled = true;
                });
        }

        /// <summary>
        /// Resaves an anchor.
        /// </summary>
        /// <param name="anchorDesignController">The controller.</param>
        private void Adjust_OnResave(AnchorDesignController anchorDesignController)
        {
            _moveAnchor.enabled = false;
            _adjustAnchor.enabled = false;
            
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
            _adjustAnchor.enabled = false;

            // unlock + hide
            _moveController = anchorDesignController;
            _moveController.Anchor.Schema.Set("locked", false);
            _moveController.Renderer.gameObject.SetActive(false);

            // open move menu
            _moveAnchor.Initialize(_design.Config);
            _moveAnchor.enabled = true;
        }
        
        /// <summary>
        /// Verbose logging.
        /// </summary>
        [Conditional("LOGGING_VERBOSE")]
        private void Verbose(string message, params object[] replacements)
        {
            Log.Info(this, message, replacements);
        }
    }
}