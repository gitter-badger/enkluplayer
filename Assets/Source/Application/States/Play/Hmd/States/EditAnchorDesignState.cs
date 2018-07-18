using System;
using System.Collections.Generic;
using System.Diagnostics;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.IUX;
using UnityEditor.VersionControl;
using UnityEngine;
using Object = UnityEngine.Object;
using Void = CreateAR.Commons.Unity.Async.Void;

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
        /// Used here and there to iterate.
        /// </summary>
        private readonly List<AnchorDesignController> _scratchList = new List<AnchorDesignController>();
        
        /// <summary>
        /// Design controller.
        /// </summary>
        private HmdDesignController _design;
        
        /// <summary>
        /// Menu for placing new anchors.
        /// </summary>
        private PlaceAnchorController _placeNewAnchor;

        /// <summary>
        /// Menu for moving anchors.
        /// </summary>
        private PlaceAnchorController _moveAnchor;

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
            
            // place anchor menu
            {
                _placeNewAnchor = unityRoot.AddComponent<PlaceAnchorController>();
                _placeNewAnchor.OnCancel += PlaceNewAnchor_OnCancel;
                _placeNewAnchor.OnOk += PlaceNewAnchor_OnOk;
                _placeNewAnchor.enabled = false;
            }

            // move anchor menu
            {
                _moveAnchor = unityRoot.AddComponent<PlaceAnchorController>();
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
            Object.Destroy(_placeNewAnchor);
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
            _placeNewAnchor.enabled = false;
            _adjustAnchor.enabled = false;
            _moveAnchor.enabled = false;
        }
        
        /// <summary>
        /// Exports an anchor and updates associated data.
        /// </summary>
        /// <param name="data">The element data.</param>
        private IAsyncToken<Void> CreateAnchor(ElementData data)
        {
            var token = new AsyncToken<Void>();

            // create URI
            var url = data.Schema.Strings["src"] = string.Format(
                "/editor/app/{0}/scene/{1}/anchor/{2}",
                _design.App.Id,
                _elementUpdater.Active,
                data.Id);
            var version = data.Schema.Ints["version"] = 0;

            // create placeholder
            var placeholder = Object.Instantiate(
                _design.Config.AnchorPrefab,
                data.Schema.Vectors["position"].ToVector(),
                Quaternion.identity);
            placeholder.PlaceholderSaving();

            // cleans up after all potential code paths
            Action cleanup = () =>
            {
                Log.Info(this, "Destroying placeholder.");

                Object.Destroy(placeholder.gameObject);
            };

            // export
            Log.Info(this, "CreateAnchor() called, beginning export and upload.");

            // export it
            _provider
                .Export(data.Id, placeholder.gameObject)
                .OnSuccess(bytes =>
                {
                    Log.Info(this, "Successfully exported. Progressing to upload.");

                    // save to cache
                    _cache.Save(data.Id, version, bytes);

                    // upload
                    UploadAnchor(data, url, bytes, cleanup, 3)
                        .OnSuccess(token.Succeed)
                        .OnFailure(token.Fail);
                })
                .OnFailure(exception =>
                {
                    Log.Error(this,
                        "Could not export anchor : {0}.",
                        exception);

                    token.Fail(exception);

                    cleanup();
                });

            return token;
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
        /// Uploads an anchor for the first time.
        /// </summary>
        private IAsyncToken<Void> UploadAnchor(ElementData data, string url, byte[] bytes, Action cleanup, int retries)
        {
            var token = new AsyncToken<Void>();

            _http
                .PostFile<Trellis.Messages.UploadAnchor.Response>(
                    _http.Urls.Url(url),
                    new Commons.Unity.DataStructures.Tuple<string, string>[0],
                    ref bytes)
                .OnSuccess(response =>
                {
                    if (response.Payload.Success)
                    {
                        Log.Info(this, "Successfully uploaded anchor.");

                        // complete, now create corresponding element
                        _elementUpdater
                            .Create(data)
                            .OnSuccess(_ =>
                            {
                                Log.Info(this, "Successfully created anchor element.");

                                token.Succeed(Void.Instance);
                            })
                            .OnFailure(exception =>
                            {
                                Log.Error(this,
                                    "Could not create anchor element : {0}.",
                                    exception);

                                token.Fail(exception);
                            });
                    }
                    else
                    {
                        var error = string.Format(
                            "Anchor was uploaded but server returned an error : {0}.",
                            response.Payload.Error);
                        Log.Error(this, error);

                        token.Fail(new Exception(error));
                    }

                    // run cleanup
                    cleanup();
                })
                .OnFailure(exception =>
                {
                    Log.Warning(this,
                        "Could not upload anchor : {0}.",
                        exception);

                    if (--retries > 0)
                    {
                        Log.Info(this, "Retrying upload.");

                        UploadAnchor(data, url, bytes, cleanup, retries);
                    }
                    else
                    {
                        token.Fail(exception);

                        cleanup();
                    }
                });

            return token;
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
        /// Called when new anchor is requested.
        /// </summary>
        private void Anchors_OnNew()
        {
            _placeNewAnchor.Initialize(_design.Config);
            _placeNewAnchor.enabled = true;
        }

        /// <summary>
        /// Called when back button is pressed on anchor menu.
        /// </summary>
        private void Anchors_OnBack()
        {
            _design.ChangeState<MainDesignState>();
        }
        
        /// <summary>
        /// Called when place anchor confirms placement.
        /// </summary>
        private void PlaceNewAnchor_OnOk(ElementData data)
        {
            _placeNewAnchor.enabled = false;
            _moveAnchor.enabled = false;

            // TODO: open progress indicator

            // create anchor
            CreateAnchor(data)
                .OnSuccess(_ =>
                {
                    _design.ChangeState<MainDesignState>();
                })
                .OnFailure(exception =>
                {
                    int id;
                    _ui
                        .Open<ICommonErrorView>(new UIReference
                        {
                            UIDataId = UIDataIds.ERROR
                        }, out id)
                        .OnSuccess(el =>
                        {
                            el.Message = string.Format("There was an error creating the anchor: {0}",
                                exception.Message);
                            el.Action = "Ok";
                            el.OnOk += () =>
                            {
                                _ui.Close(id);

                                _design.ChangeState<MainDesignState>();
                            };
                        })
                        .OnFailure(ex =>
                        {
                            _design.ChangeState<MainDesignState>();

                            Log.Error(this, "Could not open error view: {0}.", ex);
                        });
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
                }); ;
        }

        /// <summary>
        /// Resaves an anchor.
        /// </summary>
        /// <param name="anchorDesignController">The controller.</param>
        private void Adjust_OnResave(AnchorDesignController anchorDesignController)
        {
            _placeNewAnchor.enabled = false;
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
        /// Called when place anchor cancels placement.
        /// </summary>
        private void PlaceNewAnchor_OnCancel()
        {
            _placeNewAnchor.enabled = false;
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