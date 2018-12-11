using System.Collections;
using CreateAR.Commons.Unity.Http;
using UnityEngine;

namespace CreateAR.EnkluPlayer.Scripting
{
    /// <summary>
    /// 
    /// </summary>
    public class NetworkJsApi
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
        
        /// <summary>
        /// Returns if there's an active connection to the internet.
        /// </summary>
        public bool online { get; private set; }
        
        /// <summary>
        /// Returns the Round Trip Time for a ping request.
        /// </summary>
        public float pingMs { get; private set; }

        /// <summary>
        /// AWS region to ping against.
        /// </summary>
        public string pingRegion
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
        public float pingInterval
        {
            get { return _pingInterval; }
            set
            {
                Stop();
                _pingInterval = value;
                Start();
            }
        }

        /// <summary>
        /// ID given to coroutines so if they overlap, they'll nicely die out.
        /// </summary>
        private int _coroutineID = 0;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="config"></param>
        /// <param name="http"></param>
        /// <param name="bootstrapper"></param>
        /// <param name="metrics"></param>
        public NetworkJsApi(
            PingConfig config,
            IHttpService http, 
            IBootstrapper bootstrapper,
            IMetricsService metrics)
        {
            _pingInterval = config.Interval;
            _pingRegion = config.Region;
            
            _http = http;
            _bootstrapper = bootstrapper;
            _pingMetric = metrics.Value(MetricsKeys.PERF_PING);

            if (config.Enabled)
            {
                Start();
            }
        }

        /// <summary>
        /// Starts the ping coroutine.
        /// </summary>
        private void Start()
        {
            _bootstrapper.BootstrapCoroutine(Ping());
        }

        /// <summary>
        /// Stops the ping couroutine. Any inflight requests will still finish.
        /// </summary>
        private void Stop()
        {
            _coroutineID++;
        }

        /// <summary>
        /// Pings AWS until stopped.
        /// </summary>
        /// <returns></returns>
        private IEnumerator Ping()
        {
            var id = _coroutineID;
            var url = string.Format("https://ec2.{0}.amazonaws.com/ping", _pingRegion);

            while (id == _coroutineID)
            {
                var startTime = Time.realtimeSinceStartup;
                var inflight = true;
                
                // Send the request
                _http.Get<string>(url).OnSuccess(httpResponse =>
                {
                    online = true;
                    pingMs = Time.realtimeSinceStartup - startTime;
                    _pingMetric.Value(pingMs);
                }).OnFailure(exception =>
                {
                    online = false;
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
        }
    }
}