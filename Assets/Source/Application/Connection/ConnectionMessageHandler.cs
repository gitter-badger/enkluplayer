using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using LightJson;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Receives messages from a connection.
    /// </summary>
    public class ConnectionMessageHandler
    {
        private const int PONG_DELTA_MS = 3000;

        /// <summary>
        /// Filters messages for router.
        /// </summary>
        private readonly MessageFilter _filter;

        /// <summary>
        /// Binds message types to C# types.
        /// </summary>
        private readonly MessageTypeBinder _binder;

        /// <summary>
        /// List we can write to.
        /// </summary>
        private readonly List<string> _messageWriteBuffer = new List<string>();

        /// <summary>
        /// List we read from.
        /// </summary>
        private readonly List<string> _messageReadBuffer = new List<string>();

        /// <summary>
        /// Last time we received a ping.
        /// </summary>
        private DateTime _lastPing = DateTime.MinValue;

        /// <summary>
        /// Last time we sent a pong.
        /// </summary>
        private DateTime _lastPong = DateTime.MinValue;

        /// <summary>
        /// Init packet.
        /// </summary>
        private WebSocketInitPacket _initPacket;

        public event Action OnHeartbeatRequested;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ConnectionMessageHandler(
            IBootstrapper bootstrapper,
            MessageFilter filter,
            MessageTypeBinder binder)
        {
            _filter = filter;
            _binder = binder;

            bootstrapper.BootstrapCoroutine(ConsumeMessages());
        }

        /// <summary>
        /// Handles messages from connection. This is expected to be called on
        /// a thread pool thread. It will be pushed to the main thread next
        /// update.
        /// </summary>
        /// <param name="message">The message.</param>
        public void OnMessage(string message)
        {
            lock (_messageWriteBuffer)
            {
                _messageWriteBuffer.Add(message);
            }
        }

        /// <summary>
        /// Processes message on main thread.
        /// </summary>
        /// <param name="message">The string message.</param>
        private void ProcessMessage(string message)
        {
            // process init
            var header = "0";
            if (message.StartsWith(header))
            {
                message = message.Substring(header.Length);

                try
                {
                    _initPacket = (WebSocketInitPacket) JsonValue
                        .Parse(message)
                        .As(typeof(WebSocketInitPacket));
                }
                catch (Exception exception)
                {
                    Log.Error(this,
                        "Could not parse init packet : {0}.",
                        exception);
                }

                LogVerbose("Received init packet.");

                return;
            }

            // process ping
            header = "40";
            if (message == header)
            {
                _lastPing = DateTime.Now;

                LogVerbose("Ping()");

                return;
            }

            // process messages
            header = "42[\"message\",";
            if (message.StartsWith(header))
            {
                message = message.Substring(header.Length);
                message = message.TrimEnd(']');
            }
            else
            {
                return;
            }

            LogVerbose("Message Received: {0}", message);

            JsonValue parsed;
            int messageType;

            try
            {
                parsed = JsonValue.Parse(message);
                messageType = parsed["type"].AsInteger;
            }
            catch (InvalidOperationException)
            {
                // invalid json
                return;
            }

            LogVerbose("\tMessage type: {0}", messageType);

            var type = _binder.ByMessageType(messageType);

            LogVerbose("\tBound type: {0}", type);

            if (null == type)
            {
                Log.Warning(this,
                    "Binder had no type for [{0}] : {1}.",
                    messageType,
                    message);

                return;
            }

            var typedMessage = parsed.As(type);

            LogVerbose("\tMessage: {0}", typedMessage);

            _filter.Publish(messageType, typedMessage);
        }

        /// <summary>
        /// Long running generator to pull messages off the write queue and push
        /// them onto the read queue.
        /// </summary>
        /// <returns></returns>
        private IEnumerator ConsumeMessages()
        {
            while (true)
            {
                lock (_messageWriteBuffer)
                {
                    if (_messageWriteBuffer.Count > 0)
                    {
                        _messageReadBuffer.AddRange(_messageWriteBuffer);
                        _messageWriteBuffer.Clear();
                    }
                }

                if (_messageReadBuffer.Count > 0)
                {
                    for (var i = 0; i < _messageReadBuffer.Count; i++)
                    {
                        ProcessMessage(_messageReadBuffer[i]);
                    }

                    _messageReadBuffer.Clear();
                }

                // ping
                var now = DateTime.Now;
                if (null != _initPacket
                    && _lastPing != DateTime.MinValue
                    && now.Subtract(_lastPing).TotalMilliseconds > _initPacket.PingInterval / 2f
                    && now.Subtract(_lastPong).TotalMilliseconds > PONG_DELTA_MS)
                {
                    _lastPong = DateTime.Now;

                    if (null != OnHeartbeatRequested)
                    {
                        OnHeartbeatRequested();
                    }
                }

                yield return null;
            }
        }

        /// <summary>
        /// Logs.
        /// </summary>
        [Conditional("LOGGING_VERBOSE")]
        private void LogVerbose(string message, params object[] replacements)
        {
            Log.Info(this, message, replacements);
        }
    }
}