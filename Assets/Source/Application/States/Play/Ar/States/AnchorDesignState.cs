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
        /// Design controller.
        /// </summary>
        private ArDesignController _design;

        /// <summary>
        /// Anchor menu.
        /// </summary>
        private AnchorMenuController _anchors;

        /// <summary>
        /// Menu for placing anchors.
        /// </summary>
        private PlaceAnchorController _placeAnchor;
        
        /// <summary>
        /// Adjusts anchors.
        /// </summary>
        private AdjustAnchorController _adjustAnchor;

        /// <summary>
        /// Distance filter.
        /// </summary>
        private readonly DistanceElementControllerFilter _distanceFilter = new DistanceElementControllerFilter();

        /// <summary>
        /// Content filter.
        /// </summary>
        private readonly TypeElementControllerFilter _anchorFilter = new TypeElementControllerFilter(typeof(WorldAnchorWidget));

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
            ArDesignController design,
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
                _placeAnchor = unityRoot.AddComponent<PlaceAnchorController>();
                _placeAnchor.OnCancel += PlaceAnchor_OnCancel;
                _placeAnchor.OnOk += PlaceAnchor_OnOk;
                _placeAnchor.enabled = false;
            }

            // adjust menu
            {
                _adjustAnchor = unityRoot.AddComponent<AdjustAnchorController>();
                _adjustAnchor.OnDelete += Adjust_OnDelete;
                _adjustAnchor.OnExit += Adjust_OnExit;
                _adjustAnchor.enabled = false;
            }
        }

        /// <inheritdoc />
        public void Enter(object context)
        {
            Log.Info(this, "Entering {0}", GetType().Name);

            _anchors.enabled = true;

            _controllers
                .Filter(_distanceFilter)
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
            _controllers
                .Remove<AnchorDesignController>()
                .Unfilter(_distanceFilter)
                .Unfilter(_anchorFilter);

            CloseAll();

            Log.Info(this, "Exited {0}", GetType().Name);
        }

        /// <summary>
        /// Closes all menus.
        /// </summary>
        private void CloseAll()
        {
            _anchors.enabled = false;
            _placeAnchor.enabled = false;
            _adjustAnchor.enabled = false;
        }

        /// <summary>
        /// Closes splash menus on all anchors.
        /// </summary>
        private void CloseSplashMenus()
        {
            _scratchList.Clear();
            _controllers.All(_scratchList);
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
            _controllers.All(_scratchList);
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
            var sceneIds = _design.Txns.TrackedScenes;
            foreach (var sceneId in sceneIds)
            {
                var root = _design.Txns.Root(sceneId);
                if (root == parent)
                {
                    return sceneId;
                }
            }

            return null;
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
            _placeAnchor.Initialize(_design.Config);
            _placeAnchor.enabled = true;
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
        private void PlaceAnchor_OnOk(ElementData data)
        {
            _placeAnchor.enabled = false;
            _anchors.enabled = true;

            // create anchor first
            var url = data.Schema.Strings["src"] = string.Format(
                "/editor/app/{0}/scene/{1}/anchor/{2}",
                _design.App.Id,
                _elementUpdater.Active,
                data.Id);
            data.Schema.Ints["version"] = 0;

            // create placeholder
            var placeholder = Object.Instantiate(
                _design.Config.AnchorPrefab,
                data.Schema.Vectors["position"].ToVector(),
                Quaternion.identity);
            placeholder.Saving();

            // cleans up after all potential code paths
            Action cleanup = () => { Object.Destroy(placeholder.gameObject); };

            // export
            Verbose("Exporting placeholder.");

            _provider
                .Export(data.Id, placeholder.gameObject)
                .OnSuccess(bytes =>
                {
                    Verbose("Successfully exported. Progressing to upload.");

                    // save to cache
                    _cache.Save(data.Id, bytes);

                    _http
                        .PostFile<Trellis.Messages.UploadAnchor.Response>(
                            _http.UrlBuilder.Url(url),
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
                        })
                        .OnFailure(exception =>
                        {
                            Log.Error(this,
                                "Could not upload anchor : {0}.",
                                exception);
                        })
                        .OnFinally(_ => cleanup());
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
        /// Called when adjust menu wants to exit.
        /// </summary>
        private void Adjust_OnExit(AnchorDesignController controller)
        {
            _adjustAnchor.enabled = false;
            _anchors.enabled = true;

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
        /// Called when place anchor cancels placement.
        /// </summary>
        private void PlaceAnchor_OnCancel()
        {
            _placeAnchor.enabled = false;
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