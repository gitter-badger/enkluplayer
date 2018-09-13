#if NETFX_CORE
using System.Text;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// IBridge implementation for UWP.
    /// </summary>
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

        /// <summary>
        /// Handles connections from the web editor.
        /// </summary>
        private readonly UwpWebsocketServer _server;

        /// <summary>
        /// Handles messages from the client.
        /// </summary>
        private BridgeMessageHandler _handler;

        /// <summary>
        /// Set to true when <c>BroadcastReady</c> is called.
        /// </summary>
        private bool _broadcastReady = false;

        /// <summary>
        /// Serializes.
        /// </summary>
        private readonly JsonSerializer _serializer = new JsonSerializer();

        /// <inheritdoc cref="IBridge"/>
        public MessageTypeBinder Binder { get { return _handler.Binder; } }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="bootstrapper">Bootstraps coroutines.</param>
        public UwpBridge(IBootstrapper bootstrapper)
        {
            _server = new UwpWebsocketServer(bootstrapper, this);
            
            _server.Listen();

            LogNetworkInfo();
        }

        /// <inheritdoc cref="IBridge"/>
        public void BroadcastReady()
        {
            _broadcastReady = true;
            
            CallMethod("ready");
        }

        /// <inheritdoc cref="IUwpWebsocketService"/>
        public void OnOpen()
        {
            Log.Info(this, "WebSocket connection opened.");

            CallMethod("init");

            if (_broadcastReady)
            {
                CallMethod("ready");
            }
        }

        /// <inheritdoc cref="IUwpWebsocketService"/>
        public void OnMessage(string message)
        {
            _handler.OnMessage(message);
        }

        /// <inheritdoc cref="IUwpWebsocketService"/>
        public void OnClose()
        {
            Log.Info(this, "WebSocket connection closed.");
        }

        /// <inheritdoc cref="IBridge"/>
        public void Initialize(BridgeMessageHandler handler)
        {
            _handler = handler;
        }

        /// <inheritdoc cref="IBridge"/>
        public void Uninitialize()
        {
            Binder.Clear();
        }
        
        /// <inheritdoc />
        public void Send(string message)
        {
            // TODO
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