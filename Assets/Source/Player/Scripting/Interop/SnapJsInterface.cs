using System;
using System.Linq;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Trellis.Messages;
using CreateAR.Trellis.Messages.TriggerSnap;
using Jint;
using Jint.Native;
using JsFunc = System.Func<Jint.Native.JsValue, Jint.Native.JsValue[], Jint.Native.JsValue>;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// JavaScript interface for snaps.
    /// </summary>
    [JsInterface("snaps")]
    public class SnapJsInterface
    {
        /// <summary>
        /// Trellis interface.
        /// </summary>
        private readonly ApiController _api;

        /// <summary>
        /// Manages user preferences.
        /// </summary>
        private readonly UserPreferenceService _preferences;

        /// <summary>
        /// Constructor.
        /// </summary>
        public SnapJsInterface(
            ApiController api,
            UserPreferenceService prefs)
        {
            _api = api;
            _preferences = prefs;
        }

        /// <summary>
        /// Triggers a snap to be taken.
        /// </summary>
        public void trigger(Engine engine, string tag)
        {
            trigger_callback(engine, tag, null);
        }

        /// <summary>
        /// Triggers a snap to be taken. Notifies the callback on success/failure.
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="callback"></param>
        public void trigger_callback(Engine engine, string tag, JsFunc callback)
        {
            Log.Info(this, "Trigger a snap.");
            
            _preferences
                .ForCurrentUser()
                .OnSuccess(prefs =>
                {
                    var getOrgToken = new AsyncToken<string>();
                    
                    // pick first organization
                    var org = prefs.Data.DeviceRegistrations.FirstOrDefault();

                    if (null != org)
                    {
                        getOrgToken.Succeed(org.OrgId);
                    }
                    else
                    {
                        // Attempt to get the org if it wasn't available?!
                        Log.Warning(this, "No organizations.");

                        _api.Organizations.GetMyOrganizations().OnSuccess(response =>
                        {
                            if (response.Payload.Success && response.Payload.Body.Length > 0)
                            {
                                getOrgToken.Succeed(response.Payload.Body[0].Id);
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
                    
                    // After org is known
                    getOrgToken.OnSuccess(orgId =>
                    {
                        Log.Info(this, "Making snap request.");

                        _api
                            .Snaps
                            .TriggerSnap(orgId, tag, new Request())
                            .OnSuccess(response =>
                            {
                                if (response.Payload.Success)
                                {
                                    Log.Info(this, "Trigger succeeded.");
                                    InvokeCallback(engine, callback, true);
                                }
                                else
                                {
                                    Log.Warning(this,
                                        "Trigger could not be sent : {0}",
                                        response.Payload.Error);
                                    InvokeCallback(engine, callback, false, response.Payload.Error);
                                }
                            })
                            .OnFailure(exception =>
                            {
                                Log.Warning(this,
                                    "Trigger could not be fired : {0}",
                                    exception);
                                InvokeCallback(engine, callback, false, exception.ToString());
                            });
                    }).OnFailure(exception =>
                    {
                        InvokeCallback(engine, callback, false, exception.ToString());
                    });
                    
                })
                .OnFailure(ex =>
                {
                    Log.Error(this, "Could not get user preferences : {0}", ex);
                    InvokeCallback(engine, callback, false, ex.ToString());
                });
        }

        /// <summary>
        /// Invokes a JsFunc with the specified parameters.
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="success"></param>
        /// <param name="msg"></param>
        private void InvokeCallback(Engine engine, JsFunc callback, bool success, string msg = "")
        {
            if (callback != null)
            {
                callback(JsValue.FromObject(engine, this), new[] { new JsValue(success), new JsValue(msg) });
            }
        }
    }
}