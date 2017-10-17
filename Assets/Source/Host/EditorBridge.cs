#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Messaging;
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
#pragma warning disable 414
            // ReSharper disable once InconsistentNaming
            public string methodName;
#pragma warning restore 414
        }

        /// <summary>
        /// Serializes.
        /// </summary>
        private readonly JsonSerializer _serializer = new JsonSerializer();
        
        /// <summary>
        /// Handles messages.
        /// </summary>
        private readonly BridgeMessageHandler _handler;

        /// <summary>
        /// For message passing.
        /// </summary>
        private readonly IMessageRouter _router;

        /// <summary>
        /// Token for init.
        /// </summary>
        private AsyncToken<Void> _initToken;

        /// <summary>
        /// WebSocket server.
        /// </summary>
        private WebSocketServer _server;

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
        /// <param name="router">Routes messages.</param>
        /// <param name="handler">Object to handle messages.</param>
        public EditorBridge(
            IBootstrapper bootstrapper,
            IMessageRouter router,
            BridgeMessageHandler handler)
        {
            _handler = handler;
            _router = router;

            // start watcher "thread" -- can persiste between goes
            bootstrapper.BootstrapCoroutine(ConsumeMessages());
        }

        /// <inheritdoc cref="IBridge"/>
        public void Initialize()
        {
            // listen for connections
            _initToken = new AsyncToken<Void>();
            _server = new WebSocketServer("ws://localhost:4649");
            _server.AddWebSocketService(
                "/bridge",
                () => this);
            _server.Start();
        }

        /// <inheritdoc cref="IBridge"/>
        public void Uninitialize()
        {
            _initToken.Abort();
            _initToken = null;

            _server.Stop(CloseStatusCode.Normal, "Application reset requested.");
            _server = null;

            lock (_messages)
            {
                _messages.Clear();
            }
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
        /// Called when we receieve a message.
        /// 
        /// NOTE: This is called in a worker thread, so it is put on a queue.
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

            Sessions.Sweep();

            Commons.Unity.Logging.Log.Info(this, "WebSocket client left.");

            // reset entire application
            _router.Publish(MessageTypes.RESET, Void.Instance);
        }

        /// <summary>
        /// Generator that consumes messages off the queue.
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Kills server.
        /// </summary>
        private void ReleaseUnmanagedResources()
        {
            _server.Stop();
        }

        /// <summary>
        /// IDisposable implementation.
        /// </summary>
        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Destructor.
        /// </summary>
        ~EditorBridge()
        {
            ReleaseUnmanagedResources();
        }
    }
}
#endif