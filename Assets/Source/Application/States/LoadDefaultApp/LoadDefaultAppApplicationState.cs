using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using CreateAR.Trellis.Messages;
using CreateAR.Trellis.Messages.GetMyApps;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// State responsible for loading the default app.
    /// </summary>
    public class LoadDefaultAppApplicationState : IState
    {
        /// <summary>
        /// Manages UI.
        /// </summary>
        private readonly IUIManager _ui;

        /// <summary>
        /// Messages interface.
        /// </summary>
        private readonly IMessageRouter _messages;
        
        /// <summary>
        /// Manages preferences.
        /// </summary>
        private readonly UserPreferenceService _preferences;

        /// <summary>
        /// Application wide configuration.
        /// </summary>
        private readonly ApplicationConfig _config;

        /// <summary>
        /// Trellis API.
        /// </summary>
        private readonly ApiController _api;

        /// <summary>
        /// Caches HTTP calls.
        /// </summary>
        private readonly HttpRequestCacher _httpCache;

        /// <summary>
        /// UI frame for popping later.
        /// </summary>
        private UIManagerFrame _frame;

        /// <summary>
        /// Id for loading screen.
        /// </summary>
        private int _loadingScreenId = -1;

        /// <summary>
        /// Id of error popup.
        /// </summary>
        private int _errorPopupId = -1;

        /// <summary>
        /// Token for getting apps.
        /// </summary>
        private IAsyncToken<Response> _getMyAppsToken;

        private UserPreferenceData _prefs;

        /// <summary>
        /// Constructor.
        /// </summary>
        public LoadDefaultAppApplicationState(
            IUIManager ui,
            IMessageRouter messages,
            UserPreferenceService preferences,
            ApplicationConfig config,
            ApiController api,
            HttpRequestCacher cache)
        {
            _ui = ui;
            _messages = messages;
            _preferences = preferences;
            _config = config;
            _api = api;
            _httpCache = cache;
        }
        
        /// <inheritdoc />
        public void Enter(object context)
        {
            Log.Info(this, "LoadDefaultAppState::Enter()");

            // UI frame
            _frame = _ui.CreateFrame();

            // loading screen
            _ui.Open<ICommonLoadingView>(new UIReference
            {
                UIDataId = UIDataIds.LOADING
            }, out _errorPopupId);

            // load preferences
            _preferences
                .ForUser(_config.Network.Credentials.UserId)
                .OnSuccess(
                    obj =>
                    {
                        _prefs = obj.Data;
                        if (string.IsNullOrEmpty(_prefs.MostRecentAppId))
                        {
                            ChooseDefaultApp();
                        }
                        else
                        {
                            LoadApp(_prefs.MostRecentAppId, _prefs.App(_prefs.MostRecentAppId));
                        }
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

            if (null != _getMyAppsToken)
            {
                _getMyAppsToken.Abort();
                _getMyAppsToken = null;
            }
        }

        /// <summary>
        /// Chooses a default app by peeking through all user's apps.
        /// </summary>
        private void ChooseDefaultApp()
        {
            Log.Info(this, "Choosing a default app.");

            _getMyAppsToken = _httpCache.Request(
                HttpRequestCacher.LoadBehavior.NetworkFirst,
                "userdata://applist",
                _api.Apps.GetMyApps);
            _getMyAppsToken
                .OnSuccess(response =>
                {
                    // pick first app
                    var apps = response.Body;
                    if (apps.Length > 0)
                    {
                        var id = apps[0].Id;
                        LoadApp(id, _prefs.App(id));
                    }
                    else
                    {
                        // user has no apps
                        _messages.Publish(MessageTypes.USER_PROFILE);
                    }
                })
                .OnFailure(exception =>
                {
                    _ui.Close(_loadingScreenId);
                    _ui
                        .Open<ErrorPopupUIView>(
                            new UIReference
                            {
                                UIDataId = UIDataIds.ERROR
                            },
                            out _errorPopupId)
                        .OnSuccess(popup =>
                        {
                            popup.Message = "Could not retrieve apps. Are you sure you're online?";
                            popup.Action = "Retry";
                            popup.OnOk += Retry_OnOk;
                        });
                });
        }

        /// <summary>
        /// Loads an app.
        /// </summary>
        /// <param name="appId">The id of the app to load.</param>
        /// <param name="app"></param>
        private void LoadApp(string appId, UserAppPreferenceData app)
        {
            _config.Play.AppId = appId;
            _config.Play.Edit = !app.Play;
            
            _messages.Publish(MessageTypes.LOAD_APP);
        }

        /// <summary>
        /// Called on retry.
        /// </summary>
        private void Retry_OnOk()
        {
            _ui.Pop();

            ChooseDefaultApp();
        }
    }
}