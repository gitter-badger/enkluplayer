using System;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.EnkluPlayer.Scripting
{
    /// <summary>
    /// JsApi for editing scenes.
    /// </summary>
    [JsInterface("edit")]
    public class EditJsApi
    {
        /// <summary>
        /// The connection.
        /// </summary>
        private readonly IConnection _connection;

        /// <summary>
        /// Manages transaction.
        /// </summary>
        private readonly IElementTxnManager _txns;

        /// <summary>
        /// Scenes.
        /// </summary>
        private readonly IAppSceneManager _scenes;

        /// <summary>
        /// Application-wide configuration.
        /// </summary>
        private readonly ApplicationConfig _config;

        /// <summary>
        /// Token for connecting.
        /// </summary>
        private AsyncToken<Void> _connectToken;

        /// <summary>
        /// Txn api.
        /// </summary>
        public TxnJsApi txns { get; private set; }

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
        public EditJsApi(
            IConnection connection,
            IElementTxnManager txnManager,
            IAppSceneManager scenes,
            ApplicationConfig config,
            TxnJsApi transactionApi)
        {
            _connection = connection;
            _config = config;
            _txns = txnManager;
            _scenes = scenes;
            txns = transactionApi;
        }

        /// <summary>
        /// Connects to Trellis.
        /// </summary>
        public void connect(Action<string> cb)
        {
            if (null == _connectToken)
            {
                var env = _config.Network.Environment;

                _connectToken = new AsyncToken<Void>();
                
                // connect + init txns
                _connection
                    .Connect(env)
                    .OnSuccess(_ => _txns
                        .Initialize(new AppTxnConfiguration
                            {
                                AppId = _config.Play.AppId,
                                AuthenticateTxns = true,
                                Scenes = _scenes
                            })
                            .OnSuccess(_connectToken.Succeed)
                            .OnFailure(ex =>
                            {
                                Log.Warning(this, "Could not initialize txn manager : {0}", ex);

                                _connectToken.Fail(ex);
                            }))
                    .OnFailure(_connectToken.Fail);
            }

            _connectToken
                .OnSuccess(_ => cb(null))
                .OnFailure(ex => cb(ex.Message));
        }
    }
}
