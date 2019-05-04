using System;
using System.Linq;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using CreateAR.EnkluPlayer.Player.Session;
using CreateAR.Trellis.Messages;
using CreateAR.Trellis.Messages.TriggerSnap;
using Source.Player.Scripting.Interop;
using Enklu.Orchid;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// JavaScript interface for snaps.
    /// </summary>
    [JsInterface("snaps")]
    public class SnapJsInterface
    {
        /// <summary>
        /// Session controller
        /// </summary>
        private readonly IPlayerSessionController _sessions;

        /// <summary>
        /// Trellis interface.
        /// </summary>
        private readonly ApiController _api;

        /// <summary>
        /// Manages user preferences.
        /// </summary>
        private readonly UserPreferenceService _preferences;

        /// <summary>
        /// Cached Org Id.
        /// </summary>
        private string _orgId = string.Empty;

        public SnapUploader uploader { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public SnapJsInterface(
            IVideoManager videoManager,
            IImageManager imageManager,
            IPlayerSessionController sessions,
            ApiController api,
            UserPreferenceService prefs)
        {
            uploader = new SnapUploader(videoManager, imageManager);
            _api = api;
            _sessions = sessions;
            _preferences = prefs;
        }

        /// <summary>
        /// Triggers a snap to be taken.
        /// </summary>
        public void trigger(string instanceId)
        {
            triggerCallback(instanceId, string.Empty, null);
        }

        /// <summary>
        /// Triggers a snap to be taken. Notifies the callback on success/failure.
        /// </summary>
        public void triggerCallback(string instanceId, string tag, IJsCallback callback)
        {
            Log.Info(this, "Trigger a snap.");

            _preferences
                .ForCurrentUser()
                .OnSuccess(prefs =>
                {
                    var getOrgToken = new AsyncToken<string>();

                    if (!string.IsNullOrEmpty(_orgId))
                    {
                        // use cached org id if already known
                        getOrgToken.Succeed(_orgId);
                    }
                    else
                    {
                        // pick first organization
                        var org = prefs.Data.DeviceRegistrations.FirstOrDefault();

                        if (null != org)
                        {
                            _orgId = org.OrgId;
                            getOrgToken.Succeed(_orgId);
                        }
                        else
                        {
                            // Attempt to get the org if it wasn't available?!
                            Log.Warning(this, "No organizations.");

                            _api.Organizations.GetMyOrganizations().OnSuccess(response =>
                            {
                                if (response.Payload.Success && response.Payload.Body.Length > 0)
                                {
                                    _orgId = response.Payload.Body[0].Id;
                                    getOrgToken.Succeed(_orgId);
                                }
                                else
                                {
                                    getOrgToken.Fail(new Exception("GetMyOrganizations success: false"));
                                }
                            }).OnFailure(exception =>
                            {
                                getOrgToken.Fail(exception);
                            });
                        }
                    }

                    // After org is known
                    getOrgToken.OnSuccess(orgId =>
                    {
                        var session = _sessions.CurrentSession;
                        var userId = null != session ? session.UserId : "";
                        var sessionId = null != session ? session.SessionId : "";
                        Log.Info(this, "Making snap request [UserId: {0}, SessionId: {1}]", userId, sessionId);

                        _api
                            .Snaps
                            .TriggerSnap(orgId, instanceId, new Request()
                            {
                                Tag = tag,
                                SessionId = sessionId,
                                UserId = userId
                            })
                            .OnSuccess(response =>
                            {
                                if (response.Payload.Success)
                                {
                                    Log.Info(this, "Trigger succeeded.");
                                    InvokeCallback(callback, true);
                                }
                                else
                                {
                                    Log.Warning(this,
                                        "Trigger could not be sent : {0}",
                                        response.Payload.Error);
                                    InvokeCallback(callback, false, response.Payload.Error);
                                }
                            })
                            .OnFailure(exception =>
                            {
                                Log.Warning(this,
                                    "Trigger could not be fired : {0}",
                                    exception);
                                InvokeCallback(callback, false, exception.ToString());
                            });
                    }).OnFailure(exception =>
                    {
                        InvokeCallback(callback, false, exception.ToString());
                    });

                })
                .OnFailure(ex =>
                {
                    Log.Error(this, "Could not get user preferences : {0}", ex);
                    InvokeCallback(callback, false, ex.ToString());
                });
        }

        /// <summary>
        /// Invokes a IJsCallback with the specified parameters.
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="success"></param>
        /// <param name="msg"></param>
        private void InvokeCallback(IJsCallback callback, bool success, string msg = "")
        {
            if (callback != null)
            {
                callback.Apply(this, success, msg);
            }
        }
    }
}