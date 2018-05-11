using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using CreateAR.Trellis.Messages;
using CreateAR.Trellis.Messages.GetMyApps;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// State for user information.
    /// </summary>
    public class UserProfileApplicationState : IState
    {
        /// <summary>
        /// App-wide config.
        /// </summary>
        private readonly ApplicationConfig _config;

        /// <summary>
        /// Api calls.
        /// </summary>
        private readonly ApiController _api;

        /// <summary>
        /// Messages.
        /// </summary>
        private readonly IMessageRouter _messages;

        /// <summary>
        /// Manages UI.
        /// </summary>
        private readonly IUIManager _ui;
        
        /// <summary>
        /// Caches http requests.
        /// </summary>
        private readonly HttpRequestCacher _http;

        /// <summary>
        /// UI frame.
        /// </summary>
        private UIManagerFrame _frame;
        
        /// <summary>
        /// Token to get my apps.
        /// </summary>
        private IAsyncToken<Response> _myAppsToken;

        /// <summary>
        /// Constructor.
        /// </summary>
        public UserProfileApplicationState(
            ApplicationConfig config,
            ApiController api,
            IMessageRouter messages,
            IFileManager files,
            IUIManager ui)
        {
            _config = config;
            _messages = messages;
            _api = api;
            _ui = ui;

            _http = new HttpRequestCacher(files);
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
            
        }

        /// <inheritdoc />
        public void Exit()
        {
            if (null != _myAppsToken)
            {
                _myAppsToken.Abort();
            }

            _frame.Release();
        }

        /// <summary>
        /// Loads proile.
        /// </summary>
        private void LoadProfile()
        {
            const string uri = "userdata://userprofile";

            _myAppsToken = _http.Request(
                HttpRequestCacher.LoadBehavior.NetworkFirst,
                uri,
                () => _api.Apps.GetMyApps());
            
            _myAppsToken
                .OnSuccess(response =>
                {
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
                                popup.OnOk += Retry_OnOk;
                            })
                            .OnFailure(ex => Log.Fatal(
                                this,
                                "Could not open error popup : {0}.",
                                ex));
                    }
                })
                .OnFailure(exception =>
                {
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
                            popup.OnOk += Retry_OnOk;
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
                .Open<IAppSelectionUIView>(new UIReference
                {
                    UIDataId = "UserProfile.AppSelection"
                })
                .OnSuccess(el =>
                {
                    el.OnAppSelected += AppSelection_OnSelected;
                    el.OnSignOut += AppSelection_OnSignOut;
                    el.Apps = apps;
                });
        }

        /// <summary>
        /// Called when the user splash controller selects an app.
        /// </summary>
        /// <param name="appId">The id of the app to load.</param>
        private void AppSelection_OnSelected(string appId)
        {
            _config.Play.AppId = appId;

            _messages.Publish(MessageTypes.LOAD_APP);
        }
        
        /// <summary>
        /// Called when the user requests to sign out.
        /// </summary>
        private void AppSelection_OnSignOut()
        {
            _messages.Publish(MessageTypes.SIGNOUT);
        }

        /// <summary>
        /// Called to try retrieving apps.
        /// </summary>
        private void Retry_OnOk()
        {
            _ui.Pop();
            
            LoadProfile();
        }
    }
}