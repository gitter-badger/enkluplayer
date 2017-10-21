#if NETFX_CORE
using System;
using Windows.Networking;
using Windows.Networking.Connectivity;
using CreateAR.Commons.Unity.Logging;
using UnityEngine;
using UnityEngine.UI;

namespace CreateAR.SpirePlayer
{
    public class UwpBridge : IBridge, IUwpWebsocketService
    {
        private readonly UwpWebsocketServer _server;
        private readonly BridgeMessageHandler _handler;

        public MessageTypeBinder Binder { get; }

        public UwpBridge(BridgeMessageHandler handler)
        {
            _handler = handler;
            _server = new UwpWebsocketServer(this);

            Binder = new MessageTypeBinder();

            _server.Listen();
        }

        public void BroadcastReady()
        {
            var announce = GameObject.Find("Announcement");
            var text = announce.GetComponent<Text>();

            foreach (var localHostName in NetworkInformation.GetHostNames())
            {
                if (localHostName.IPInformation != null)
                {
                    if (localHostName.Type == HostNameType.Ipv4)
                    {
                        text.text = localHostName.ToString();
                        Log.Info(this, localHostName.ToString());
                        break;
                    }
                }
            }
        }

        public void OnOpen()
        {
            Log.Info(this, "WebSocket connection opened.");
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
    }
}
#endif