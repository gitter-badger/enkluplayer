#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using CreateAR.Spire;
using WebSocketSharp;
using WebSocketSharp.Server;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// <c>IBridge</c> implementation in the Unity Editor.
    /// </summary>
    public class EditorBridge : WebSocketBehavior, IBridge, IDisposable
    {
        /// <summary>
        /// Represents a method on the webpage.
        /// </summary>
        private class Method
        {
            /// <summary>
            /// Name of the method.
            /// </summary>
            public string methodName;
        }

        /// <summary>
        /// Serializes.
        /// </summary>
        private readonly JsonSerializer _serializer = new JsonSerializer();

        /// <summary>
        /// Bootstraps coroutines.
        /// </summary>
        private readonly IBootstrapper _bootstrapper;

        /// <summary>
        /// Handles messages.
        /// </summary>
        private readonly IBridgeMessageHandler _handler;
        
        /// <summary>
        /// WebSocket server.
        /// </summary>
        private readonly WebSocketServer _server;

        /// <summary>
        /// Token for init.
        /// </summary>
        private readonly AsyncToken<Void> _initToken = new AsyncToken<Void>();

        /// <summary>
        /// Messages received but not yet processed.
        /// </summary>
        private readonly List<string> _messages = new List<string>();

        /// <summary>
        /// Allows binding between message type and C# type.
        /// </summary>
        public MessageTypeBinder Binder { get { return _handler.Binder; } }

        /// <summary>
        /// Creates a new <c>EditorBridge</c>.
        /// </summary>
        /// <param name="bootstrapper">Bootstraps coroutines.</param>
        /// <param name="handler">Object to handle messages.</param>
        public EditorBridge(
            IBootstrapper bootstrapper,
            IBridgeMessageHandler handler)
        {
            _handler = handler;
            _bootstrapper = bootstrapper;

            // start watcher "thread"
            _bootstrapper.BootstrapCoroutine(ConsumeMessages());

            // listen for connections
            _server = new WebSocketServer("ws://localhost:4649");
            _server.AddWebSocketService(
                "/bridge",
                () => this);
            _server.Start();
        }

        /// <inheritdoc cref="IBridge"/>
        public void BroadcastReady()
        {
            _initToken.OnSuccess(_ => CallMethod("ready"));
        }
            
        /// <summary>
        /// Called when a client joins.
        /// </summary>
        protected override void OnOpen()
        {
            base.OnOpen();
                
            Commons.Unity.Logging.Log.Info(this, "WebSocket client joined.");

            CallMethod("init");

            _initToken.Succeed(Void.Instance);
        }

        /// <summary>
        /// Called when a client sends a message.
        /// </summary>
        /// <param name="event"></param>
        protected override void OnMessage(MessageEventArgs @event)
        {
            base.OnMessage(@event);

            lock (_messages)
            {
                _messages.Add(@event.Data);
            }
        }

        /// <summary>
        /// Called when a client leaves.
        /// </summary>
        /// <param name="event"></param>
        protected override void OnClose(CloseEventArgs @event)
        {
            base.OnClose(@event);

            Commons.Unity.Logging.Log.Info(this, "WebSocket client left.");
        }

        private IEnumerator ConsumeMessages()
        {
            while (true)
            {
                string[] messages = null;
                lock (_messages)
                {
                    if (_messages.Count > 0)
                    {
                        messages = _messages.ToArray();
                        _messages.Clear();
                    }
                }

                if (null != messages)
                {
                    for (int i = 0, len = messages.Length; i < len; i++)
                    {
                        _handler.OnMessage(messages[i]);
                    }
                }

                yield return null;
            }
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
            
            SendAsync(
                payload,
                result =>
                {
                    if (!result)
                    {
                        Commons.Unity.Logging.Log.Error(this, "Could not send message.");
                    }
                });
        }

        private void ReleaseUnmanagedResources()
        {
            _server.Stop();
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        ~EditorBridge()
        {
            ReleaseUnmanagedResources();
        }
    }
}
#endif