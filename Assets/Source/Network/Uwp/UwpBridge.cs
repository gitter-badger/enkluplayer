#if NETFX_CORE
using CreateAR.Spire;

namespace CreateAR.SpirePlayer
{
    public class UwpBridge : IBridge, IUwpWebsocketService
    {
        private readonly UwpWebsocketServer _server = new UwpWebsocketServer();
        private readonly BridgeMessageHandler _handler;

        public MessageTypeBinder Binder { get; }

        public UwpBridge(BridgeMessageHandler handler)
        {
            _handler = handler;

            Binder = new MessageTypeBinder();

            _server.Listen();
        }

        public void BroadcastReady()
        {
            
        }

        public void OnOpen()
        {
            
        }

        public void OnMessage(string message)
        {
            
        }

        public void OnClose()
        {
            
        }
    }
}
#endif