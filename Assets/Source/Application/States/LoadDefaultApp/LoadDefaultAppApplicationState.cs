using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using CreateAR.Trellis.Messages;
using CreateAR.Trellis.Messages.GetMyApps;

namespace CreateAR.SpirePlayer
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
        /// Id of error popup.
        /// </summary>
        private int _errorPopupId = -1;

        /// <summary>
        /// Token for getting apps.
        /// </summary>
        private IAsyncToken<Response> _getMyAppsToken;

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
            
            // load preferences
            _preferences
                .ForUser(_config.Network.Credentials.UserId)
                .OnSuccess(
                    obj =>
                    {
                        var prefs = obj.Data;
                        if (string.IsNullOrEmpty(prefs.MostRecentAppId))
                        {
                            ChooseDefaultApp();
                        }
                        else
                        {
                            LoadApp(prefs.MostRecentAppId);
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
            _ui.Close(_errorPopupId);
            _errorPopupId = -1;

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
                        LoadApp(apps[0].Id);
                    }
                    else
                    {
                        // user has no apps
                        _messages.Publish(MessageTypes.USER_PROFILE);
                    }
                })
                .OnFailure(exception =>
                {
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
        private void LoadApp(string appId)
        {
            _config.Play.AppId = appId;
            _config.Play.Edit = false;
            
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