using System.Linq;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Trellis.Messages;
using CreateAR.Trellis.Messages.TriggerSnap;

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
        public void trigger(string tag)
        {
            Log.Info(this, "Trigger a snap.");

            _preferences
                .ForCurrentUser()
                .OnSuccess(prefs =>
                {
                    // pick first organization
                    var org = prefs.Data.DeviceRegistrations.FirstOrDefault();
                    if (null == org)
                    {
                        Log.Warning(this, "No organizations.");
                        return;
                    }

                    Log.Info(this, "Making snap request.");

                    _api
                        .Snaps
                        .TriggerSnap(org.OrgId, tag, new Request())
                        .OnSuccess(response =>
                        {
                            if (response.Payload.Success)
                            {
                                Log.Info(this, "Trigger succeeded.");
                            }
                            else
                            {
                                Log.Warning(this,
                                    "Trigger could not be sent : {0}",
                                    response.Payload.Error);
                            }
                        })
                        .OnFailure(exception => Log.Warning(this,
                            "Trigger could not be fired : {0}",
                            exception));
                })
                .OnFailure(ex => Log.Error(this, "Could not get user preferences : {0}", ex));
        }
    }
}