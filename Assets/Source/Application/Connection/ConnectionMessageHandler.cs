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
        /// <summary>
        /// MS between pongs.
        /// </summary>
        private const int PONG_DELTA_MS = 3000;

        /// <summary>
        /// MS until we timeout.
        /// </summary>
        private const int TIMEOUT_MS = 10000;

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

        /// <summary>
        /// Called when the handler requests a heartbeat to be sent.
        /// </summary>
        public event Action OnHeartbeatRequested;

        /// <summary>
        /// Called when the handler times out.
        /// </summary>
        public event Action OnTimeout;

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
            if (ProcessInitMessage(message))
            {
                return;
            }

            if (ProcessHeartbeatMessage(message))
            {
                return;
            }

            if (ProcessTrellisMessage(message))
            {
                return;
            }

            Log.Warning(this, "Received message we don't know how to handle : {0}.", message);
        }

        private bool ProcessInitMessage(string message)
        {
            const string header = "0";
            if (!message.StartsWith(header))
            {
                return false;
            }

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

            LogVerbose("Received init.");

            return true;

        }

        private bool ProcessHeartbeatMessage(string message)
        {
            const string header = "40";
            if (message != header)
            {
                return false;
            }

            _lastPing = DateTime.Now;

            LogVerbose("Received ping.");

            return true;

        }

        private bool ProcessTrellisMessage(string message)
        {
            const string header = "42[\"message\",";
            if (!message.StartsWith(header))
            {
                return false;
            }

            message = message.Substring(header.Length);
            message = message.TrimEnd(']');

            LogVerbose("Received Trellis message : {0}", message);

            // try to parse using 'type'-- these are messages from the editor
            JsonValue parsed;

            try
            {
                parsed = JsonValue.Parse(message);
            }
            catch (InvalidOperationException exception)
            {
                // invalid json
                Log.Error(this, "Received invalid JSON : {0}.", exception);

                return true;
            }

            if (HandleEditorMessage(parsed))
            {
                return true;
            }

            if (HandleAssetMessage(parsed))
            {
                return true;
            }

            if (HandleScriptMessage(parsed))
            {
                return true;
            }

            return false;
        }

        private bool HandleEditorMessage(JsonValue parsed)
        {
            var messageType = parsed["type"].AsInteger;
            if (0 != messageType)
            {
                var type = _binder.ByMessageType(messageType);
                if (null != type)
                {
                    LogVerbose("\tMessage is bound to type: {0}", type);

                    object typedMessage = null;
                    try
                    {
                        typedMessage = parsed.As(type);
                    }
                    catch (Exception exception)
                    {
                        Log.Error(this, "Could not parse message as {0} : {1}.",
                            type.Name,
                            exception);
                    }

                    if (null != typedMessage)
                    {
                        LogVerbose("\tHandling message : {0}", typedMessage);

                        _filter.Publish(messageType, typedMessage);

                        return true;
                    }
                }
            }

            return false;
        }

        private bool HandleAssetMessage(JsonValue parsed)
        {
            var messageType = parsed["messageType"].AsString;
            switch (messageType)
            {
                case "assetcreation":
                {
                    AssetData asset;
                    try
                    {
                        var assetData = parsed["payload"]["asset"];

                        // first, parse stats
                        var statsString = assetData["stats"].AsString;
                        var stats = (AssetStatsData) JsonValue
                            .Parse(statsString)
                            .As(typeof(AssetStatsData));
                        assetData["stats"] = new JsonValue();

                        asset = (AssetData) assetData.As(typeof(AssetData));
                        asset.Stats = stats;
                    }
                    catch (Exception exception)
                    {
                        Log.Warning(this, "Could not parse asset creation event : {0}.", exception);
                        return false;
                    }

                    _filter.Publish(
                        MessageTypes.RECV_ASSET_ADD,
                        new AssetAddEvent
                        {
                            Asset = asset
                        });

                    return true;
                }
                case "assetstats":
                {
                    AssetStatsEvent @event;
                    try
                    {
                        // first, parse stats
                        var statsString = parsed["payload"]["stats"].AsString;
                        var stats = (AssetStatsData) JsonValue
                            .Parse(statsString)
                            .As(typeof(AssetStatsData));

                        @event = new AssetStatsEvent
                        {
                            Id = parsed["payload"]["assetId"].AsString,
                            Stats = stats
                        };
                    }
                    catch (Exception exception)
                    {
                        Log.Warning(this, "Could not parse asset stats event : {0}.", exception);
                        return false;
                    }

                    _filter.Publish(
                        MessageTypes.RECV_ASSET_UPDATE_STATS,
                        @event);

                    return true;
                }
                case "assetdeleted":
                {
                    var id = parsed["payload"]["assetId"].AsString;

                    _filter.Publish(
                        MessageTypes.RECV_ASSET_REMOVE,
                        new AssetDeleteEvent
                        {
                            Id = id
                        });

                    return true;
                }
                case "assetupdate":
                {
                    // discard
                    return true;
                }
            }

            return false;
        }

        private bool HandleScriptMessage(JsonValue parsed)
        {
            var messageType = parsed["messageType"].AsString;
            switch (messageType)
            {
                case "scriptcreated":
                {
                    ScriptData payload;
                    try
                    {
                        payload = (ScriptData) parsed["payload"].As(typeof(ScriptData));
                    }
                    catch (Exception exception)
                    {
                        Log.Error(this, "Script creation event couldn't be parsed : {0}.",
                            exception);
                        return true;
                    }

                    _filter.Publish(
                        MessageTypes.RECV_SCRIPT_ADD,
                        new ScriptAddEvent
                        {
                            Script = payload
                        });

                    return true;
                    }
                case "scriptupdated":
                {
                    ScriptData payload;
                    try
                    {
                        payload = (ScriptData) parsed["payload"].As(typeof(ScriptData));
                    }
                    catch (Exception exception)
                    {
                        Log.Error(this, "Script update event couldn't be parsed : {0}.",
                            exception);
                        return true;
                    }

                    _filter.Publish(
                        MessageTypes.RECV_SCRIPT_UPDATE,
                        new ScriptUpdateEvent
                        {
                            Script = payload
                        });

                    return true;
                }
                case "scriptdeleted":
                {
                    var id = parsed["payload"].AsString;

                    _filter.Publish(
                        MessageTypes.RECV_SCRIPT_REMOVE,
                        new ScriptRemoveEvent
                        {
                            Id = id
                        });

                    return true;
                }
            }

            return false;
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

                if (_lastPing != DateTime.MinValue
                    && now.Subtract(_lastPing).TotalMilliseconds > TIMEOUT_MS)
                {
                    if (null != OnTimeout)
                    {
                        OnTimeout();
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