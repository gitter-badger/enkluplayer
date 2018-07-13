using System;
using System.Collections.Generic;
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
    public class AnchorDesignState : IArDesignState
    {
        /// <summary>
        /// Controller group tag for anchors.
        /// </summary>
        private const string TAG_ANCHOR = "anchor";

        /// <summary>
        /// Manages controllers on elements.
        /// </summary>
        private readonly IElementControllerManager _controllers;

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
        /// Used here and there to iterate.
        /// </summary>
        private readonly List<AnchorDesignController> _scratchList = new List<AnchorDesignController>();
        
        /// <summary>
        /// Content filter.
        /// </summary>
        private readonly TypeElementControllerFilter _anchorFilter = new TypeElementControllerFilter(typeof(WorldAnchorWidget));

        /// <summary>
        /// Design controller.
        /// </summary>
        private HmdDesignController _design;

        /// <summary>
        /// Anchor menu.
        /// </summary>
        private AnchorMenuController _anchors;

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
        public AnchorDesignState(
            IElementControllerManager controllers,
            IHttpService http,
            IWorldAnchorCache cache,
            IWorldAnchorProvider provider,
            IElementUpdateDelegate elementUpdater)
        {
            _controllers = controllers;
            _http = http;
            _cache = cache;
            _provider = provider;
            _elementUpdater = elementUpdater;
        }

        /// <inheritdoc />
        public void Initialize(
            HmdDesignController design,
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
                dynamicRoot.AddChild(_anchors.Root);
            }

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
            Object.Destroy(_anchors);
            Object.Destroy(_placeNewAnchor);
            Object.Destroy(_adjustAnchor);
            Object.Destroy(_moveAnchor);
        }

        /// <inheritdoc />
        public void Enter(object context)
        {
            Log.Info(this, "Entering {0}", GetType().Name);

            _anchors.enabled = true;

            _controllers
                .Group(TAG_ANCHOR)
                .Filter(_anchorFilter)
                .Add<AnchorDesignController>(new AnchorDesignController.AnchorDesignControllerContext
                {
                    AppId = _design.App.Id,
                    SceneId = SceneIdForElement,
                    Config = _design.Config,
                    Http = _http,
                    Cache = _cache,
                    Provider = _provider,
                    OnAdjust = Controller_OnAdjust
                });
        }

        /// <inheritdoc />
        public void Update(float dt)
        {
            
        }

        /// <inheritdoc />
        public void Exit()
        {
            _controllers.Destroy(TAG_ANCHOR);

            CloseAll();

            Log.Info(this, "Exited {0}", GetType().Name);
        }

        /// <summary>
        /// Closes all menus.
        /// </summary>
        private void CloseAll()
        {
            _anchors.enabled = false;
            _placeNewAnchor.enabled = false;
            _adjustAnchor.enabled = false;
            _moveAnchor.enabled = false;
        }

        /// <summary>
        /// Closes splash menus on all anchors.
        /// </summary>
        private void CloseSplashMenus()
        {
            _scratchList.Clear();
            _controllers.Group(TAG_ANCHOR).All(_scratchList);
            for (int i = 0, len = _scratchList.Count; i < len; i++)
            {
                _scratchList[i].CloseSplash();
            }
        }

        /// <summary>
        /// Opens splash menus on all anchors.
        /// </summary>
        private void OpenSplashMenus()
        {
            _scratchList.Clear();
            _controllers.Group(TAG_ANCHOR).All(_scratchList);
            for (int i = 0, len = _scratchList.Count; i < len; i++)
            {
                _scratchList[i].OpenSplash();
            }
        }

        /// <summary>
        /// Scene id for element.
        /// </summary>
        /// <param name="element">Element.</param>
        /// <returns></returns>
        private string SceneIdForElement(Element element)
        {
            // find root
            var parent = element;
            while (true)
            {
                if (null != parent.Parent)
                {
                    parent = parent.Parent;
                }
                else
                {
                    break;
                }
            }

            // find id of root
            var sceneIds = _design.Scenes.All;
            foreach (var sceneId in sceneIds)
            {
                var root = _design.Scenes.Root(sceneId);
                if (root == parent)
                {
                    return sceneId;
                }
            }

            return null;
        }

        /// <summary>
        /// Exports an anchor and updates associated data.
        /// </summary>
        /// <param name="data">The element data.</param>
        private void CreateAnchor(ElementData data)
        {
            // create anchor first
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
            placeholder.Saving();

            // cleans up after all potential code paths
            Action cleanup = () => Object.Destroy(placeholder.gameObject);

            // export
            Verbose("Exporting placeholder.");

            _provider
                .Export(data.Id, placeholder.gameObject)
                .OnSuccess(bytes =>
                {
                    Verbose("Successfully exported. Progressing to upload.");

                    // save to cache
                    _cache.Save(data.Id, version, bytes);

                    UploadAnchor(data, url, bytes, cleanup, 3);
                })
                .OnFailure(exception =>
                {
                    Log.Error(this,
                        "Could not export anchor : {0}.",
                        exception);

                    cleanup();
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
            controller.Renderer.Saving();

            // export
            Verbose("Reexporting anchor.");

            _provider
                .Export(anchor.Id, anchor.GameObject)
                .OnSuccess(bytes =>
                {
                    Verbose("Successfully exported. Progressing to upload.");

                    // save to cache
                    _cache.Save(anchor.Id, version, bytes);

                    ReuploadAnchor(controller, bytes, anchor, version, 3);
                })
                .OnFailure(exception =>
                {
                    Log.Error(this,
                        "Could not export anchor : {0}.",
                        exception);

                    controller.Renderer.Error();
                });
        }

        /// <summary>
        /// Uploads an anchor for the first time.
        /// </summary>
        private void UploadAnchor(ElementData data, string url, byte[] bytes, Action cleanup, int retries)
        {
            _http
                .PostFile<Trellis.Messages.UploadAnchor.Response>(
                    _http.Urls.Url(url),
                    new Commons.Unity.DataStructures.Tuple<string, string>[0],
                    ref bytes)
                .OnSuccess(response =>
                {
                    if (response.Payload.Success)
                    {
                        Verbose("Successfully uploaded anchor.");

                        // complete, now create corresponding element
                        _elementUpdater
                            .Create(data)
                            .OnSuccess(_ => Verbose("Successfully created anchor element."))
                            .OnFailure(exception => Log.Error(this,
                                "Could not create anchor : {0}.",
                                exception));
                    }
                    else
                    {
                        Log.Error(this,
                            "Anchor upload error : {0}.",
                            response.Payload.Error);
                    }

                    cleanup();
                })
                .OnFailure(exception =>
                {
                    Log.Error(this,
                        "Could not upload anchor : {0}.",
                        exception);

                    if (--retries > 0)
                    {
                        UploadAnchor(data, url, bytes, cleanup, retries);
                    }
                    else
                    {
                        cleanup();
                    }
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
                        Verbose("Successfully uploaded anchor.");

                        // complete, now send out network update
                        _elementUpdater.Update(anchor, "src", url);
                        _elementUpdater.Update(anchor, "version", version);
                        _elementUpdater.Update(anchor, "position", anchor.Schema.Get<Vec3>("position").Value);
                        _elementUpdater.FinalizeUpdate(anchor);

                        controller.Renderer.Ready();
                    }
                    else
                    {
                        Log.Error(this,
                            "Anchor upload error : {0}.",
                            response.Payload.Error);

                        controller.Renderer.Error();
                    }
                })
                .OnFailure(exception =>
                {
                    Log.Error(this,
                        "Could not upload anchor : {0}.",
                        exception);

                    if (--retries > 0)
                    {
                        Log.Info(this, "Retry creating anchor.");

                        ReuploadAnchor(controller, bytes, anchor, version, retries);
                    }
                    else
                    {
                        controller.Renderer.Error();
                    }
                });
        }

        /// <summary>
        /// Called by anchor to open adjust menu.
        /// </summary>
        /// <param name="controller"></param>
        private void Controller_OnAdjust(AnchorDesignController controller)
        {
            _anchors.enabled = false;
            
            CloseSplashMenus();

            _adjustAnchor.Initialize(controller);
        }

        /// <summary>
        /// Called when new anchor is requested.
        /// </summary>
        private void Anchors_OnNew()
        {
            _anchors.enabled = false;
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
            _anchors.enabled = true;

            CreateAnchor(data);
        }

        /// <summary>
        /// Called when the move menu wants to finish moving.
        /// </summary>
        /// <param name="data">Data for the element.</param>
        private void MoveAnchor_OnOk(ElementData data)
        {
            _moveAnchor.enabled = false;
            _anchors.enabled = true;

            // move into position and reimport
            var position = data.Schema.Vectors["position"];
            _moveController.Anchor.Schema.Set("position", position);
            _moveController.Anchor.GameObject.transform.position = position.ToVector();
            _moveController.Renderer.gameObject.SetActive(true);

            _moveController.OpenSplash();

            ResaveAnchor(_moveController);
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
            _adjustAnchor.enabled = false;
            _anchors.enabled = true;
            _moveAnchor.enabled = false;
            _placeNewAnchor.enabled = false;

            OpenSplashMenus();
        }

        /// <summary>
        /// Called when the adjust menu wants to delete an element.
        /// </summary>
        private void Adjust_OnDelete(AnchorDesignController controller)
        {
            _adjustAnchor.enabled = false;

            _elementUpdater
                .Destroy(controller.Element)
                .OnFinally(_ =>
                {
                    _anchors.enabled = true;
                    OpenSplashMenus();
                })
                .OnFailure(exception =>
                {
                    Log.Error(this,
                        "Could not delete element : {0}.",
                        exception);
                });
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
            _anchors.enabled = true;

            anchorDesignController.OpenSplash();

            ResaveAnchor(anchorDesignController);
        }

        /// <summary>
        /// Reloads an anchor.
        /// </summary>
        /// <param name="anchorDesignController">The controller.</param>
        private void Adjust_OnReload(AnchorDesignController anchorDesignController)
        {
            _placeNewAnchor.enabled = false;
            _moveAnchor.enabled = false;
            _adjustAnchor.enabled = false;
            _anchors.enabled = true;

            anchorDesignController.OpenSplash();
            anchorDesignController.Anchor.Reload();
            anchorDesignController.ChangeState<AnchorLoadingState>();
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