#if NETFX_CORE
using System.Text;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;

namespace CreateAR.SpirePlayer
{
    public class UwpBridge : IBridge, IUwpWebsocketService
    {
        /// <summary>
        /// Represents a method on the webpage.
        /// </summary>
        private class Method
        {
            /// <summary>
            /// Name of the method.
            /// </summary>
#pragma warning disable 414
            // ReSharper disable once InconsistentNaming
            public string methodName;
#pragma warning restore 414
        }

        private readonly UwpWebsocketServer _server;
        private readonly BridgeMessageHandler _handler;

        private bool _broadcastReady = false;

        /// <summary>
        /// Serializes.
        /// </summary>
        private readonly JsonSerializer _serializer = new JsonSerializer();

        public MessageTypeBinder Binder { get { return _handler.Binder; } }

        public UwpBridge(
            IBootstrapper bootstrapper,
            BridgeMessageHandler handler)
        {
            _handler = handler;
            _server = new UwpWebsocketServer(bootstrapper, this);
            
            _server.Listen();

            LogNetworkInfo();
        }

        public void BroadcastReady()
        {
            _broadcastReady = true;
            
            CallMethod("ready");
        }

        public void OnOpen()
        {
            Log.Info(this, "WebSocket connection opened.");

            CallMethod("init");

            if (_broadcastReady)
            {
                CallMethod("ready");
            }
        }

        public void OnMessage(string message)
        {
            _handler.OnMessage(message);
        }

        public void OnClose()
        {
            Log.Info(this, "WebSocket connection closed.");
        }

        public void Initialize()
        {
            
        }

        public void Uninitialize()
        {
            
        }

        /// <summary>
        /// Sends a message to connected hosts.
        /// </summary>
        /// <param name="methodName">The message type to send.</param>
        private void CallMethod(string methodName)
        {
            byte[] bytes;
            _serializer.Serialize(
                new Method
                {
                    methodName = methodName
                },
                out bytes);

            var payload = Encoding.UTF8.GetString(bytes);

            _server.Send(payload);
        }

        /// <summary>
        /// Logs IPv4 info.
        /// </summary>
        private void LogNetworkInfo()
        {
            foreach (var localHostName in Windows.Networking.Connectivity.NetworkInformation.GetHostNames())
            {
                if (localHostName.IPInformation != null)
                {
                    if (localHostName.Type == Windows.Networking.HostNameType.Ipv4)
                    {
                        Log.Info(this, "IP : " + localHostName.ToString());
                    }
                }
            }
        }
    }
}
#endif