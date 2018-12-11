using System;

namespace CreateAR.EnkluPlayer.Scripting
{
    /// <summary>
    /// 
    /// </summary>
    public class NetworkJsApi
    {
        /// <summary>
        /// Underlying network management this wraps.
        /// </summary>
        private NetworkConnectivity _networkConnectivity;

        public bool enabled
        {
            get { return _networkConnectivity.Enabled; }
            set { _networkConnectivity.Enabled = value; }
        }
        
        /// <summary>
        /// Returns if there's an active connection to the internet.
        /// </summary>
        public bool online
        {
            get { return _networkConnectivity.Online; }
        }

        /// <summary>
        /// Returns the Round Trip Time for a ping request.
        /// </summary>
        public float pingMs
        {
            get { return _networkConnectivity.PingMs; }
        }

        /// <summary>
        /// AWS region to ping against.
        /// </summary>
        public string pingRegion
        {
            get { return _networkConnectivity.PingRegion; }
            set { _networkConnectivity.PingRegion = value; }
        }

        /// <summary>
        /// Interval to ping, measured in seconds.
        /// </summary>
        public float pingInterval
        {
            get { return _networkConnectivity.PingInterval; }
            // Add a slight minimum so scripting can't spam the network too much.
            set { _networkConnectivity.PingInterval = Math.Max(1, value); }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="config"></param>
        /// <param name="http"></param>
        /// <param name="bootstrapper"></param>
        /// <param name="metrics"></param>
        public NetworkJsApi(NetworkConnectivity networkConnectivity)
        {
            _networkConnectivity = networkConnectivity;
        }
    }
}