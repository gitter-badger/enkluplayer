using System;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.IUX;
using UnityEngine;
using Object = UnityEngine.Object;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Creates a new anchor.
    /// </summary>
    public class NewAnchorDesignState : IArDesignState
    {
        /// <summary>
        /// Manages UI.
        /// </summary>
        private readonly IUIManager _ui;

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
        /// Design controller.
        /// </summary>
        private HmdDesignController _design;

        /// <summary>
        /// UI frame.
        /// </summary>
        private UIManagerFrame _frame;

        /// <summary>
        /// Constructor.
        /// </summary>
        public NewAnchorDesignState(
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
        public void Enter(object context)
        {
            _frame = _ui.CreateFrame();

            // open placement menu
            int id;
            _ui
                .Open<PlaceAnchorUIView>(new UIReference
                {
                    UIDataId = "Anchor.Place"
                }, out id)
                .OnSuccess(el =>
                {
                    el.OnOk += Place_OnOk;
                    el.OnCancel += () => _design.ChangeState<MainDesignState>();
                    el.Initialize(_design.Config);
                });
        }

        /// <inheritdoc />
        public void Update(float dt)
        {
            // 
        }

        /// <inheritdoc />
        public void Exit()
        {
            _frame.Release();
        }

        /// <inheritdoc />
        public void Initialize(HmdDesignController designer, GameObject unityRoot, Element dynamicRoot, Element staticRoot)
        {
            _design = designer;
        }

        /// <inheritdoc />
        public void Uninitialize()
        {
            // 
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
        /// Called when user wishes to place an anchor in a spot.
        /// </summary>
        /// <param name="data">The ElementData to save.</param>
        private void Place_OnOk(ElementData data)
        {
            // kill menu
            _ui.Pop();

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
    }
}