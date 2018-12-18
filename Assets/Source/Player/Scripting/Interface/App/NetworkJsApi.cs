using System;

namespace CreateAR.EnkluPlayer.Scripting
{
    /// <summary>
    /// Provides information about the network to scripting.
    /// </summary>
    public class NetworkJsApi
    {
        /// <summary>
        /// Underlying network management this wraps.
        /// </summary>
        private readonly AwsPingController _awsPingController;
        
        /// <summary>
        /// Whether pings are being sent or not.
        /// </summary>
        public bool pingEnabled
        {
            get { return _awsPingController.Enabled; }
            set { _awsPingController.Enabled = value; }
        }
        
        /// <summary>
        /// Returns if there's an active connection to the internet.
        /// </summary>
        public bool isOnline
        {
            get { return _awsPingController.Online; }
        }

        /// <summary>
        /// Returns the Round Trip Time for a ping request.
        /// </summary>
        public float pingMs
        {
            get { return _awsPingController.PingMs; }
        }

        /// <summary>
        /// AWS region to ping against.
        /// </summary>
        public string pingRegion
        {
            get { return _awsPingController.PingRegion; }
            set { _awsPingController.PingRegion = value; }
        }

        /// <summary>
        /// Interval to ping, measured in seconds.
        /// </summary>
        public float pingInterval
        {
            get { return _awsPingController.PingInterval; }
            // Add a slight minimum so scripting can't spam the network too much.
            set { _awsPingController.PingInterval = Math.Max(1, value); }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public NetworkJsApi(AwsPingController awsPingController)
        {
            _awsPingController = awsPingController;
        }
    }
}