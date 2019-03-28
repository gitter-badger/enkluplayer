using System;
using System.Collections.Generic;
using System.Linq;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using CreateAR.Trellis.Messages;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Service that watches for updates to user preferences.
    /// </summary>
    public class UserPreferenceService : ApplicationService
    {
        /// <summary>
        /// Prefix for preferences URI.
        /// </summary>
        public const string PREFERENCES_PREFIX = "login://Preferences/";

        /// <summary>
        /// Manages files.
        /// </summary>
        private readonly IFileManager _files;

        /// <summary>
        /// Application-wide configuration.
        /// </summary>
        private readonly ApplicationConfig _config;

        /// <summary>
        /// Api Controller.
        /// </summary>
        private readonly ApiController _api;

        /// <summary>
        /// Unsubscribe delegate for listening to Play.
        /// </summary>
        private Action _unsubPlay;

        /// <summary>
        /// Backing variable for Preferences property.
        /// </summary>
        private readonly Dictionary<string, IAsyncToken<SynchronizedObject<UserPreferenceData>>> _preferences = new Dictionary<string, IAsyncToken<SynchronizedObject<UserPreferenceData>>>();
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public UserPreferenceService(
            MessageTypeBinder binder,
            IMessageRouter messages,
            IFileManager files,
            ApiController api,
            ApplicationConfig config)
            : base(binder, messages)
        {
            _files = files;
            _api = api;
            _config = config;
        }

        /// <summary>
        /// Retrieves user preferences for the current user. This cannot fail.
        /// </summary>
        /// <returns></returns>
        public IAsyncToken<SynchronizedObject<UserPreferenceData>> ForCurrentUser()
        {
            return ForUser(_config.Network.Credentials.UserId);
        }

        /// <summary>
        /// Retrieves user preferences. This cannot fail.
        /// </summary>
        /// <param name="userId">User id.</param>
        public IAsyncToken<SynchronizedObject<UserPreferenceData>> ForUser(string userId)
        {
            IAsyncToken<SynchronizedObject<UserPreferenceData>> prefs;
            if (!_preferences.TryGetValue(userId, out prefs))
            {
                prefs = _preferences[userId] = Load(userId);
            }

            return prefs.Token();
        }

        /// <inheritdoc />
        public override void Start()
        {
            base.Start();
            
            _unsubPlay = _messages.Subscribe(
                MessageTypes.LOAD_APP,
                message =>
                {
                    var appId = _config.Play.AppId;
                    var userId = _config.Network.Credentials.UserId;

                    ForUser(userId)
                        .OnSuccess(prefs =>
                        {
                            prefs.Queue((data, next) =>
                            {
                                data.MostRecentAppId = appId;

                                next(data);
                            });
                        })
                        .OnFailure(exception => Log.Warning(
                            this,
                            "Could not save last loaded app : {0}.",
                            exception));
                });
        }

        /// <inheritdoc />
        public override void Stop()
        {
            base.Stop();

            _unsubPlay();
        }

        /// <summary>
        /// Loads a user's preference data.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <returns></returns>
        private IAsyncToken<SynchronizedObject<UserPreferenceData>> Load(string userId)
        {
            var fileToken = new AsyncToken<SynchronizedObject<UserPreferenceData>>();

            _files
                .Get<UserPreferenceData>(Uri(userId))
                .OnSuccess(file => fileToken.Succeed(new SynchronizedObject<UserPreferenceData>(
                    file.Data,
                    SaveAndContinue)))
                .OnFailure(exception =>
                {
                    var prefs = new UserPreferenceData
                    {
                        UserId = userId
                    };

                    _files
                        .Set(Uri(userId), prefs)
                        .OnSuccess(file => fileToken.Succeed(new SynchronizedObject<UserPreferenceData>(
                            prefs,
                            SaveAndContinue)))
                        .OnFailure(fileToken.Fail);
                });
            
            // update with org id when we can
            fileToken
                .OnSuccess(prefData =>
                {
                    if (prefData.Data.OrgIds == null || 0 == prefData.Data.OrgIds.Length)
                    {
                        Log.Info(this, "Missing organization data. Fetching from Trellis.");

                        GetOrgs()
                            .OnSuccess(orgIds => prefData.Queue((prefs, next) =>
                            {
                                prefs.OrgIds = orgIds;

                                next(prefs);
                            }))
                            .OnFailure(ex => Log.Error(this, "Could not get org data: {0}.", ex));
                    }
                })
                .OnFailure(ex => Log.Error(this, "Could not get org data: {0}.", ex));

            return fileToken;
        }

        /// <summary>
        /// Saves the data and continues.
        /// </summary>
        /// <param name="data">The data to save.</param>
        /// <param name="next">Continue Action.</param>
        private void SaveAndContinue(UserPreferenceData data, Action next)
        {
            _files
                .Set(Uri(data.UserId), data)
                .OnSuccess(file => Log.Info(this, "Successfully saved user preferences for {0}.", data.UserId))
                .OnFailure(exception => Log.Error(this, "Could not save UserPreferenceData : {0}.", exception))
                .OnFinally(_ => next());
        }

        /// <summary>
        /// Retrieves a URI for a user's preferences.
        /// </summary>
        /// <param name="userId">The user id of the user.</param>
        /// <returns></returns>
        private static string Uri(string userId)
        {
            return PREFERENCES_PREFIX + userId;
        }
        
        /// <summary>
        /// Retrieves all organizations for this user from Trellis.
        /// </summary>
        /// <returns></returns>
        private IAsyncToken<string[]> GetOrgs()
        {
            if (DeviceHelper.IsWebGl())
            {
                return new AsyncToken<string[]>(new string[0]);
            }

            var token = new AsyncToken<string[]>();

            // get list of organizations
            _api
                .Organizations
                .GetMyOrganizations()
                .OnSuccess(response =>
                {
                    if (response.Payload.Success)
                    {
                        var orgIds = response.Payload.Body.Select(org => org.Id).ToArray();
                        token.Succeed(orgIds);
                    }
                    else
                    {
                        token.Fail(new Exception(response.Payload.Error));
                    }
                })
                .OnFailure(token.Fail);

            return token;
        }
    }
}