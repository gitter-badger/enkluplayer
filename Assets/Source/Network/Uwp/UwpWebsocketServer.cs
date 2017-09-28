#if NETFX_CORE
using System;
using System.Collections.Generic;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using CreateAR.Commons.Unity.Logging;

namespace CreateAR.SpirePlayer
{
    public interface IUwpWebsocketService
    {
        void OnOpen();
        void OnMessage(string message);
        void OnClose();
    }

    public class UwpWebsocketServer
    {
        private StreamSocketListener _listener;

        public int Port { get; set; }

        public UwpWebsocketServer()
        {
            Port = 4649;
        }

        public void Listen()
        {
            _listener = new StreamSocketListener();
            _listener.Control.NoDelay = true;
            _listener.ConnectionReceived += Listener_OnConnectionReceived;

            Start();
        }

        public UwpWebsocketServer AddService(
            string prefix,
            IUwpWebsocketService service)
        {
            //
            return this;
        }

        private async void Start()
        {
            await _listener.BindServiceNameAsync(Port.ToString());

            // listening!
            Log.Info(this,
                "Listening on port {0}.",
                _listener.Information.LocalPort);
        }

        private readonly List<StreamSocket> _controllers = new List<StreamSocket>();

        private void Listener_OnConnectionReceived(
            StreamSocketListener sender,
            StreamSocketListenerConnectionReceivedEventArgs args)
        {
            var socket = args.Socket;
            _controllers.Add(socket);

            Log.Info(this,
                "Connection received from {0}.",
                socket.Information.RemoteHostName.DisplayName);

            WaitForData(socket);
        }

        private async void WaitForData(StreamSocket socket)
        {
            var reader = new DataReader(socket.InputStream);
            var stringHeader = await reader.LoadAsync(4);

            if (stringHeader == 0)
            {
                Log.Info(this,
                    "Disconnected from {0}.",
                    socket.Information.RemoteHostName.DisplayName);
                return;
            }

            var length = reader.ReadInt32();
            var numBytes = await reader.LoadAsync((uint)length);
            var message = reader.ReadString(numBytes);

            Log.Info(this,
                "Received from {0}: {1}.",
                socket.Information.RemoteHostName.DisplayName,
                message);
            
            // wait for more data
            WaitForData(socket);
        }
    }
}
#endif