using System;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using LightJson;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Service that updates remove device resource information.
    /// </summary>
    public class DeviceResourceUpdateService : ApplicationService
    {
        /// <summary>
        /// Payload for a device update.
        /// </summary>
        private class UpdatePayload
        {
            /// <summary>
            /// Contains meta.
            /// </summary>
            [JsonName("meta")]
            public DeviceResourceMeta Meta;
        }

        /// <summary>
        /// Configuration for entire application.
        /// </summary>
        private readonly ApplicationConfig _config;

        /// <summary>
        /// Provider implementation.
        /// </summary>
        private readonly IDeviceMetaProvider _provider;
        
        /// <summary>
        /// Http implementation.
        /// </summary>
        private readonly IHttpService _http;

        /// <summary>
        /// Organizations registered to this user.
        /// </summary>
        private DeviceRegistration[] _registrations;

        /// <summary>
        /// Time of last update.
        /// </summary>
        private DateTime _lastUpdate = DateTime.MaxValue;

        /// <summary>
        /// True iff initialized.
        /// </summary>
        private bool _isInitialized;

        /// <summary>
        /// Constructor.
        /// </summary>
        public DeviceResourceUpdateService(
            ApplicationConfig config,
            IDeviceMetaProvider provider,
            IHttpService http,
            MessageTypeBinder binder,
            IMessageRouter messages)
            : base(binder, messages)
        {
            _config = config;
            _provider = provider;
            _http = http;
        }

        public void Initialize(DeviceRegistration[] registrations)
        {
            _registrations = registrations;
            _lastUpdate = DateTime.Now;
            
            SendUpdates();

            _isInitialized = true;
        }

        public override void Update(float dt)
        {
            base.Update(dt);

            var now = DateTime.Now;
            if (_isInitialized
                && _lastUpdate.Subtract(now).TotalMilliseconds > _config.Conductor.BatteryUpdateDeltaMs)
            {
                _lastUpdate = now;

                SendUpdates();
            }
        }

        private void SendUpdates()
        {
            Log.Info(this, "Sending device resource update.");

            var meta = _provider.Meta();

            foreach (var registration in _registrations)
            {
                var url = _http.Urls.Url(string.Format(
                    "trellis://org/{0}/device/{1}",
                    registration.OrgId,
                    registration.DeviceId
                ));

                Log.Info(this, "\t" + url);
                _http
                    .Put<Trellis.Messages.UpdateOrganizationDevice.Response>(
                        url,
                        new UpdatePayload
                        {
                            Meta = meta
                        })
                    .OnFailure(exception => Log.Error(this, "Could not update device meta : {0}", exception));
            }
        }
    }
}