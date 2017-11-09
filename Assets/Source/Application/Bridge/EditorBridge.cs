#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Server;
using LogLevel = WebSocketSharp.LogLevel;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// <c>IBridge</c> implementation in the Unity Editor.
    /// </summary>
    public class EditorBridge : IBridge, IDisposable
    {
        /// <summary>
        /// Service for connected clients.
        /// </summary>
        private class BridgeService : WebSocketBehavior
        {
            /// <summary>
            /// Called when a client joins.
            /// </summary>
            public event Action<BridgeService> OnClientJoined;

            /// <summary>
            /// Called when a client sends a message.
            /// </summary>
            public event Action<BridgeService, MessageEventArgs> OnMessageReceived;

            /// <summary>
            /// Called when a client leaves.
            /// </summary>
            public event Action<BridgeService> OnClientLeft;

            /// <summary>
            /// Sends a payload.
            /// </summary>
            /// <param name="payload">Payload to send.</param>
            public void SendMessage(string payload)
            {
                SendAsync(
                    payload,
                    success =>
                    {
                        if (!success)
                        {
                            Commons.Unity.Logging.Log.Error(this,
                                "Could not send message.");
                        }
                    });
            }

            /// <summary>
            /// Called when a client joins.
            /// </summary>
            protected override void OnOpen()
            {
                base.OnOpen();

                Commons.Unity.Logging.Log.Info(this, "WebSocket client joined.");

                if (null != OnClientJoined)
                {
                    OnClientJoined(this);
                }
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

                if (null != OnMessageReceived)
                {
                    OnMessageReceived(this, @event);
                }
            }

            /// <summary>
            /// Called when a client leaves.
            /// 
            /// NOTE: This is called in a worker thread, so it is put on a queue.
            /// </summary>
            /// <param name="event"></param>
            protected override void OnClose(CloseEventArgs @event)
            {
                base.OnClose(@event);
                
                Commons.Unity.Logging.Log.Info(this, "WebSocket client left.");

                if (null != OnClientLeft)
                {
                    OnClientLeft(this);
                }
            }
        }

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
        /// Routes messages.
        /// </summary>
        private readonly IMessageRouter _router;

        /// <summary>
        /// Handles messages.
        /// </summary>
        private readonly BridgeMessageHandler _handler;
        
        /// <summary>
        /// WebSocket server.
        /// </summary>
        private readonly WebSocketServer _server;

        /// <summary>
        /// True iff we should broadcast ready.
        /// </summary>
        private bool _broadcastReady = false;
        
        /// <summary>
        /// Messages received but not yet processed.
        /// </summary>
        private readonly List<string> _messages = new List<string>();

        /// <summary>
        /// True iff client left.
        /// </summary>
        private bool _clientLeft = false;

        /// <summary>
        /// Joined service.
        /// </summary>
        private BridgeService _service = null;

        /// <summary>
        /// Allows binding between message type and C# type.
        /// </summary>
        public MessageTypeBinder Binder { get { return _handler.Binder; } }

        /// <summary>
        /// Creates a new <c>EditorBridge</c>.
        /// </summary>
        /// <param name="router">Routes messages.</param>
        /// <param name="bootstrapper">Bootstraps coroutines.</param>
        /// <param name="handler">Object to handle messages.</param>
        public EditorBridge(
            IMessageRouter router,
            IBootstrapper bootstrapper,
            BridgeMessageHandler handler)
        {
            _router = router;
            _handler = handler;
            
            // start watcher "thread" -- can persiste between goes
            bootstrapper.BootstrapCoroutine(ConsumeMessages());

            // create new server
            _server = new WebSocketServer("ws://localhost:4649");
            _server.Log.Level = LogLevel.Warn;
            _server.Log.Output = (data, message) =>
            {
                Debug.Log(string.Format("WS[{0}] = {1}", message, data));
            };

            // create service factory
            _server.AddWebSocketService(
                "/bridge",
                () =>
                {
                    var service = new BridgeService();
                    service.OnClientJoined += Service_OnClientJoined;
                    service.OnMessageReceived += Service_OnMessageReceived;
                    service.OnClientLeft += Service_OnClientLeft;
                    
                    return service;
                });
            _server.Start();
        }

        /// <summary>
        /// IDisposable implementation.
        /// </summary>
        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc cref="IBridge"/>
        public void Initialize()
        {
            
        }

        /// <inheritdoc cref="IBridge"/>
        public void Uninitialize()
        {
            _service = null;
            _broadcastReady = false;
            _clientLeft = false;
            
            lock (_messages)
            {
                _messages.Clear();
            }

            Binder.Clear();
        }

        /// <inheritdoc cref="IBridge"/>
        public void BroadcastReady()
        {
            _broadcastReady = true;

            // send to ready services
            if (null != _service)
            {
                CallMethod("ready", _service);
            }
        }

        /// <summary>
        /// Called when a client joins a service.
        /// </summary>
        /// <param name="service">The service associated with the user.</param>
        private void Service_OnClientJoined(BridgeService service)
        {
            CallMethod("init", service);

            if (_broadcastReady)
            {
                CallMethod("ready", service);
            }
            else
            {
                _service = service;
            }
        }

        /// <summary>
        /// Called when a message has been received by a service.
        /// 
        /// This is called from a different thread.
        /// </summary>
        /// <param name="service">The service associated with the user.</param>
        /// <param name="event">The message receieved.</param>
        private void Service_OnMessageReceived(BridgeService service, MessageEventArgs @event)
        {
            lock (_messages)
            {
                _messages.Add(@event.Data);
            }
        }

        /// <summary>
        /// Called when a user has left a service.
        /// </summary>
        /// <param name="service">The service associated with the user.</param>
        private void Service_OnClientLeft(BridgeService service)
        {
            _clientLeft = true;
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

                if (_clientLeft)
                {
                    _router.Publish(MessageTypes.RESTART, Void.Instance);
                }

                yield return null;
            }
        }

        /// <summary>
        /// Sends a message to connected hosts.
        /// </summary>
        /// <param name="methodName">The message type to send.</param>
        /// <param name="service">Optional service to send to.</param>
        private void CallMethod(string methodName, BridgeService service)
        {
            Log.Info(this, "{0}()", methodName);

            byte[] bytes;
            _serializer.Serialize(
                new Method
                {
                    methodName = methodName
                },
                out bytes);

            var payload = Encoding.UTF8.GetString(bytes);
            
            service.SendMessage(payload);
        }

        /// <summary>
        /// Kills server.
        /// </summary>
        private void ReleaseUnmanagedResources()
        {
            _server.Stop();
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