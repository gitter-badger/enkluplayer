using System.Diagnostics;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Loads and manages an app.
    /// </summary>
    public class AppController : IAppController
    {
        /// <summary>
        /// Pipe for all element updates.
        /// </summary>
        private readonly IElementTxnManager _txns;
        
        /// <inheritdoc />
        public string Id { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public AppController(IElementTxnManager txns)
        {
            _txns = txns;
        }
        
        /// <inheritdoc />
        public IAsyncToken<Void> Initialize(string appId, PlayModeConfig config)
        {
            Id = appId;

            var token = new AsyncToken<Void>();

            LogVerbose("Initialize().");
            
            _txns
                .Initialize(appId)
                .OnSuccess(_ =>
                {
                    LogVerbose("Txns initialized.");
                    
                    token.Succeed(Void.Instance);
                })
                .OnFailure(token.Fail);
            
            return token;
        }

        /// <inheritdoc />
        public void Uninitialize()
        {
            _txns.Uninitialize();
        }
        
        /// <summary>
        /// Logging.
        /// </summary>
        [Conditional("VERBOSE_LOGGING")]
        private void LogVerbose(string message, params object[] replacements)
        {
            Log.Info(this, message, replacements);
        }
    }
}