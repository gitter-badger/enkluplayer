using System;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using Newtonsoft.Json;

namespace CreateAR.EnkluPlayer
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
            [JsonProperty("meta")]
            public DeviceResourceMeta Meta { get; set; }
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

        /// <summary>
        /// Preps the service for sending meta.
        /// </summary>
        /// <param name="registrations">Device registrations to update.</param>
        public void Initialize(DeviceRegistration[] registrations)
        {
            _registrations = registrations;
            _lastUpdate = DateTime.Now;
            
            SendUpdates();

            _isInitialized = true;
        }

        /// <inheritdoc />
        public override void Update(float dt)
        {
            base.Update(dt);

            var now = DateTime.Now;
            if (_isInitialized
                && now.Subtract(_lastUpdate).TotalMilliseconds > _config.Conductor.BatteryUpdateDeltaMs)
            {
                _lastUpdate = now;

                SendUpdates();
            }
        }

        /// <summary>
        /// Sends updates to backend.
        /// </summary>
        private void SendUpdates()
        {
            var meta = _provider.Meta();

            Log.Info(this, "Sending device resource update : {0}.", meta);

            foreach (var registration in _registrations)
            {
                var url = _http.Urls.Url(string.Format(
                    "trellis://org/{0}/device/{1}",
                    registration.OrgId,
                    registration.DeviceId
                ));
                
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