using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using CreateAR.SpirePlayer.IUX;
using CreateAR.Trellis.Messages;
using CreateAR.Trellis.Messages.GetMyApps;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Creates a new anchor.
    /// </summary>
    public class AppListViewDesignState : IArDesignState
    {
        /// <summary>
        /// App-wide config.
        /// </summary>
        private readonly ApplicationConfig _config;

        /// <summary>
        /// Messages.
        /// </summary>
        private readonly IMessageRouter _messages;

        /// <summary>
        /// Search UI.
        /// </summary>
        private IAppSearchUIView _searchUi;

        /// <summary>
        /// Manages UI.
        /// </summary>
        private readonly IUIManager _ui;

        /// <summary>
        /// Design controller.
        /// </summary>
        private HmdDesignController _design;

        /// <summary>
        /// UI frame.
        /// </summary>
        private UIManagerFrame _frame;

        /// <summary>
        /// Caches http requests.
        /// </summary>
        private readonly HttpRequestCacher _http;

        /// <summary>
        /// Token to get my apps.
        /// </summary>
        private IAsyncToken<Response> _myAppsToken;

        /// <summary>
        /// Api calls.
        /// </summary>
        private readonly ApiController _api;

        /// <summary>
        /// Stack id for private app selection view.
        /// </summary>
        private int _privateAppsId;

        /// <summary>
        /// Token for search.
        /// </summary>
        private IAsyncToken<HttpResponse<Trellis.Messages.SearchPublishedApps.Response>> _appSearchToken;

        /// <summary>
        /// Constructor.
        /// </summary>
        public AppListViewDesignState(HttpRequestCacher cache,
            ApiController api,
            ApplicationConfig config,
            IMessageRouter messages,
            IUIManager ui)
        {
            _http = cache;
            _ui = ui;
            _api = api;
            _config = config;
            _messages = messages;
        }

        /// <inheritdoc />
        public void Enter(object context)
        {
            Log.Info(this, "Entered {0}.", GetType().Name);
            _frame = _ui.CreateFrame();
            LoadProfile();
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
        /// Loads profile.
        /// </summary>
        private void LoadProfile()
        {
            int loadingId;
            _ui.Open<IUIElement>(new UIReference
            {
                UIDataId = UIDataIds.LOADING
            }, out loadingId);

            const string uri = "userdata://userprofile";

            _myAppsToken = _http.Request(
                HttpRequestCacher.LoadBehavior.NetworkFirst,
                uri,
                () => _api.Apps.GetMyApps());

            _myAppsToken
                .OnSuccess(response =>
                {
                    _ui.Close(loadingId);

                    if (response.Success)
                    {
                        Log.Info(this, "Loaded UserProfileCacheData.");

                        OpenAppSelection(response.Body);
                    }
                    else
                    {
                        Log.Error(this, "Server refused to get my apps : {0}.", response.Error);

                        int errorId;
                        _ui
                            .Open<ICommonErrorView>(
                                new UIReference
                                {
                                    UIDataId = UIDataIds.ERROR
                                },
                                out errorId)
                            .OnSuccess(popup =>
                            {
                                popup.Message = "Could not load your apps. Are you sure you're online?";
                                popup.Action = "Retry";
                                popup.OnOk += () =>
                                {
                                    _ui.Close(errorId);

                                    LoadProfile();
                                };
                            })
                            .OnFailure(ex => Log.Fatal(
                                this,
                                "Could not open error popup : {0}.",
                                ex));
                    }
                })
                .OnFailure(exception =>
                {
                    _ui.Pop();

                    Log.Error(this, "Could not get my apps : {0}.", exception);

                    int errorId;
                    _ui
                        .Open<ICommonErrorView>(
                            new UIReference
                            {
                                UIDataId = UIDataIds.ERROR
                            },
                            out errorId)
                        .OnSuccess(popup =>
                        {
                            popup.Message = "Could not load your apps. Are you sure you're online?";
                            popup.Action = "Retry";
                            popup.OnOk += () =>
                            {
                                _ui.Close(errorId);

                                LoadProfile();
                            };
                        })
                        .OnFailure(ex => Log.Fatal(
                            this,
                            "Could not open error popup : {0}.",
                            ex));
                });
        }

        /// <summary>
        /// Opens the splash.
        /// </summary>
        /// <param name="apps"></param>
        private void OpenAppSelection(Body[] apps)
        {
            _ui
                .Open<HmdAppSelectionUIView>(new UIReference
                {
                    UIDataId = "UserProfile.AppSelection"
                }, out _privateAppsId)
                .OnSuccess(el =>
                {
                    el.OnAppSelected += AppSelection_OnSelected;
                    el.OnSignOut += BackTo_MainMenu;
                    el.OnPublicApps += AppSelection_OnPublicApps;

                    el.Apps = apps;
                })
                .OnFailure(ex => Log.Error(this, "Could not open AppSelection : {0}.", ex));
        }

        /// <summary>
        /// Called when the user splash controller selects an app.
        /// </summary>
        /// <param name="appId">The id of the app to load.</param>
        private void AppSelection_OnSelected(string appId)
        {
            LoadApp(appId);
        }

        /// <summary>
        /// Loads an app.
        /// </summary>
        /// <param name="appId">Id of the app to load.</param>
        private void LoadApp(string appId)
        {
            _config.Play.AppId = appId;

            _messages.Publish(MessageTypes.LOAD_APP);
        }

        /// <summary>
        /// Called when the user selects back.
        /// </summary>
        private void BackTo_MainMenu()
        {
            _design.ChangeState<MainDesignState>();
        }

        /// <summary>
        /// Called when the user requests access to public apps.
        /// </summary>
        private void AppSelection_OnPublicApps()
        {
            _ui
                .Open<IAppSearchUIView>(new UIReference
                {
                    UIDataId = "UserProfile.AppSearch"
                })
                .OnSuccess(el =>
                {
                    _searchUi = el;

                    _searchUi.OnAppSelected += AppSearch_OnSelected;
                    _searchUi.OnPrivateApps += AppSearch_OnPrivateApps;
                    _searchUi.OnQueryUpdated += AppSearch_OnQueryUpdated;
                })
                .OnFailure(exception => Log.Error(this, "Could not open AppSearch view : {0}", exception));
        }

        /// <summary>
        /// Called when the user requests to load an app.
        /// </summary>
        /// <param name="appId">Id of the app to load.</param>
        private void AppSearch_OnSelected(string appId)
        {
            LoadApp(appId);
        }

        /// <summary>
        /// Called when the user requests to login.
        /// </summary>
        private void AppSearch_OnPrivateApps()
        {
            _ui.Reveal(_privateAppsId);
        }

        /// <summary>
        /// Called when the user updates the search query.
        /// </summary>
        /// <param name="query">The search query.</param>
        private void AppSearch_OnQueryUpdated(string query)
        {
            if (null != _appSearchToken)
            {
                _appSearchToken.Abort();
            }

            _appSearchToken = _api
                .PublishedApps
                .SearchPublishedApps(query)
                .OnSuccess(response =>
                {
                    _searchUi.Init(response.Payload.Body);
                })
                .OnFailure(exception =>
                {
                    Log.Error(this, "Could not search apps : {0}", exception);

                    // TODO: show error
                });
        }
    }

}