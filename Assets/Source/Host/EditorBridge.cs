#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Server;
using Logger = WebSocketSharp.Logger;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// <c>IBridge</c> implementation in the Unity Editor.
    /// </summary>
    public class EditorBridge : IBridge, IDisposable
    {
        private class BridgeService : WebSocketBehavior
        {
            public event Action<BridgeService> OnClientJoined;
            public event Action<BridgeService, MessageEventArgs> OnMessageReceived;
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
        /// List of joined services.
        /// </summary>
        private readonly List<BridgeService> _joinedServices = new List<BridgeService>();

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
            BridgeMessageHandler handler)
        {
            _handler = handler;
            
            // start watcher "thread" -- can persiste between goes
            bootstrapper.BootstrapCoroutine(ConsumeMessages());

            // create new server
            _server = new WebSocketServer("ws://localhost:4649");
            _server.Log.Level = LogLevel.Trace;
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
            _broadcastReady = false;
            
            lock (_messages)
            {
                _messages.Clear();
            }
        }

        /// <inheritdoc cref="IBridge"/>
        public void BroadcastReady()
        {
            _broadcastReady = true;

            // send to ready services
            foreach (var service in _joinedServices)
            {
                CallMethod("ready", service);
            }
            _joinedServices.Clear();
        }

        private void Service_OnClientJoined(BridgeService service)
        {
            CallMethod("init", service);

            if (_broadcastReady)
            {
                CallMethod("ready", service);
            }
            else
            {
                _joinedServices.Add(service);
            }
        }

        private void Service_OnMessageReceived(BridgeService service, MessageEventArgs @event)
        {
            lock (_messages)
            {
                _messages.Add(@event.Data);
            }
        }

        private void Service_OnClientLeft(BridgeService service)
        {
            _joinedServices.Add(service);
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
        /// <param name="service">Optional service to send to.</param>
        private void CallMethod(string methodName, BridgeService service = null)
        {
            byte[] bytes;
            _serializer.Serialize(
                new Method
                {
                    methodName = methodName
                },
                out bytes);

            var payload = Encoding.UTF8.GetString(bytes);

            if (null == service)
            {
                _server.WebSocketServices.BroadcastAsync(
                    payload,
                    () =>
                    {
                        //
                    });
            }
            else
            {
                service.SendMessage(payload);
            }
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