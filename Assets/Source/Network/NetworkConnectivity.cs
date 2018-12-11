using System.Collections;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using UnityEngine;

namespace CreateAR.EnkluPlayer
{
    public class NetworkConnectivity
    {
        /// <summary>
        /// Dependencies.
        /// </summary>
        private IHttpService _http;
        private IBootstrapper _bootstrapper;
        
        /// <summary>
        /// Metric to report ping via.
        /// </summary>
        private ValueMetric _pingMetric;
        
        /// <summary>
        /// Backing variables.
        /// </summary>
        private float _pingInterval;
        private string _pingRegion;
        private bool _enabled;
        
        /// <summary>
        /// ID given to coroutines so if they overlap, they'll nicely die out.
        /// </summary>
        private int _coroutineID = 0;
        
        /// <summary>
        /// Whether a ping request is running or not.
        /// </summary>
        public bool Enabled
        {
            get { return _enabled; }
            set 
            {
                if (_enabled && !value)
                {
                    Stop();
                } 
                else if (!_enabled && value)
                {
                    Start();
                }

                _enabled = value;
            }
        }
        
        /// <summary>
        /// Returns if there's an active connection to the internet.
        /// </summary>
        public bool Online { get; private set; }
        
        /// <summary>
        /// Returns the Round Trip Time for the last ping request.
        /// </summary>
        public float PingMs { get; private set; }
        
        /// <summary>
        /// AWS region to ping against.
        /// </summary>
        public string PingRegion
        {
            get { return _pingRegion; }
            set
            {
                Stop();
                _pingRegion = value;
                Start();
            }
        }

        /// <summary>
        /// Interval to ping, measured in seconds.
        /// </summary>
        public float PingInterval
        {
            get { return _pingInterval; }
            set
            {
                Stop();
                _pingInterval = value;
                Start();
            }
        }
        
        public NetworkConnectivity(
            NetworkConfig config, 
            IHttpService http, 
            IBootstrapper bootstrapper,
            IMetricsService metrics)
        {
            _http = http;
            _bootstrapper = bootstrapper;
            _pingMetric = metrics.Value(MetricsKeys.PERF_PING);

            var pingConfig = config.Ping;
            _pingInterval = pingConfig.Interval;
            _pingRegion = pingConfig.Region;
            Enabled = pingConfig.Enabled;
        }
        
        /// <summary>
        /// Starts the ping coroutine.
        /// </summary>
        public void Start()
        {
            _bootstrapper.BootstrapCoroutine(Ping());
        }

        /// <summary>
        /// Stops the ping couroutine. Any inflight requests will still finish.
        /// </summary>
        public void Stop()
        {
            _coroutineID++;
        }

        /// <summary>
        /// Pings AWS until stopped.
        /// </summary>
        /// <returns></returns>
        private IEnumerator Ping()
        {
            Log.Info(this, "Starting ping. Interval: {0} Region: {1}", _pingInterval, _pingRegion);
            
            var id = _coroutineID;
            var url = string.Format("https://ec2.{0}.amazonaws.com/ping", _pingRegion);

            while (id == _coroutineID)
            {
                var startTime = Time.realtimeSinceStartup;
                var inflight = true;
                
                // Send the request
                _http.Download(url).OnSuccess(httpResponse =>
                {
                    Online = true;
                    PingMs = (Time.realtimeSinceStartup - startTime) * 1000;
                    _pingMetric.Value(PingMs);
                }).OnFailure(exception =>
                {
                    Log.Warning(this, exception);
                    Online = false;
                }).OnFinally(_ =>
                {
                    inflight = false;
                });

                // Spin for the interval
                yield return new WaitForSeconds(_pingInterval);

                // Safety, spin if the interval < ping
                while (inflight)
                {
                    yield return null;
                }
            }
            
            Log.Info(this, "Ping stopped");
        }
    }
}