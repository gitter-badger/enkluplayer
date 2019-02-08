using System;
using System.Collections;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Messaging;
using UnityEngine;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Service that sends specific metric datapoints.
    /// </summary>
    public class MetricsUpdateService : ApplicationService
    {
        /// <summary>
        /// Configuration to use.
        /// </summary>
        private readonly MetricsDataConfig _config;

        /// <summary>
        /// Dependencies.
        /// </summary>
        private readonly IBootstrapper _bootstrapper;
        private readonly IDeviceMetaProvider _deviceMeta;

        /// <summary>
        /// Metrics.
        /// </summary>
        private readonly ValueMetric _batteryMetric;
        private readonly ValueMetric _sessionMetric;

        /// <summary>
        /// Last time that an experience was started.
        /// </summary>
        private float _lastPlayTime;

        /// <summary>
        /// Cached unsub function.
        /// </summary>
        private Action _playUnsub;

        /// <summary>
        /// Current coroutine runner. Guards against rapid service Stop/Start.
        /// </summary>
        private int _currentId;
        

        /// <summary>
        /// Constructor
        /// </summary>
        public MetricsUpdateService(
            MessageTypeBinder binder,
            IMessageRouter messages,
            ApplicationConfig config,
            IMetricsService metrics,
            IBootstrapper bootstrapper,
            IDeviceMetaProvider deviceMeta) : base(binder, messages)
        {
            _bootstrapper = bootstrapper;
            _config = config.Metrics.MetricsDataConfig;
            _deviceMeta = deviceMeta;

            _batteryMetric = metrics.Value(MetricsKeys.PERF_BATTERY);
            _sessionMetric = metrics.Value(MetricsKeys.PERF_SESSION);
        }

        /// <inheritdoc />
        public override void Start()
        {
            base.Start();

            if (_config.Enabled)
            {
                _bootstrapper.BootstrapCoroutine(UpdateData());
            }

            _playUnsub = _messages.Subscribe(MessageTypes.PLAY, UpdateSessionStart);
        }

        /// <inheritdoc />
        public override void Stop()
        {
            base.Stop();

            _playUnsub();
            _currentId++;
        }

        /// <summary>
        /// Updates the enabled metrics datapoints per the configuration's interval.
        /// </summary>
        /// <returns></returns>
        private IEnumerator UpdateData()
        {
            var id = _currentId;
            
            while (id == _currentId)
            {
                yield return new WaitForSeconds(_config.Interval);

                if (_config.Battery)
                {
                    var meta = _deviceMeta.Meta();

                    _batteryMetric.Value(meta.Battery * 100);    // Send as 0->100
                }

                if (_config.SessionDuration)
                {
                    _sessionMetric.Value((Time.realtimeSinceStartup - _lastPlayTime) / 60); // Send as minutes
                }
                
                // TODO: Add temperature reporting
            }
        }

        /// <summary>
        /// Resets the cached experience start time.
        /// </summary>
        /// <param name="payload"></param>
        private void UpdateSessionStart(object payload)
        {
            _lastPlayTime = Time.realtimeSinceStartup;
        }
    }
}