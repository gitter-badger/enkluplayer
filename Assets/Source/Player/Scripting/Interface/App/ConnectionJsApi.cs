using System;
using CreateAR.Commons.Unity.Async;
using Jint;
using Jint.Native;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.EnkluPlayer.Scripting
{
    /// <summary>
    /// Scripting API for connections.
    /// </summary>
    public class ConnectionJsApi
    {
        /// <summary>
        /// The connection.
        /// </summary>
        private readonly IConnection _connection;

        /// <summary>
        /// Application-wide configuration.
        /// </summary>
        private readonly ApplicationConfig _config;

        /// <summary>
        /// Token for connecting.
        /// </summary>
        private IAsyncToken<Void> _connectToken;

        /// <summary>
        /// True iff we are connected to Trellis.
        /// </summary>
        public bool isConnected
        {
            get { return _connection.IsConnected; }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public ConnectionJsApi(
            IConnection connection,
            ApplicationConfig config)
        {
            _connection = connection;
            _config = config;
        }

        /// <summary>
        /// Connects to Trellis.
        /// </summary>
        public void connect(Engine engine, Func<JsValue, JsValue[], JsValue> cb)
        {
            if (null == _connectToken)
            {
                var env = _config.Network.Environment;
                _connectToken = _connection.Connect(env);
            }

            _connectToken
                .OnSuccess(_ => cb(
                    JsValue.FromObject(engine, this),
                    new JsValue[0]))
                .OnFailure(ex => cb(
                    JsValue.FromObject(engine, this),
                    new[] { new JsValue(ex.Message) }));
        }
    }
}