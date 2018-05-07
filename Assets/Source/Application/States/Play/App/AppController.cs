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
        /// Loads app data.
        /// </summary>
        private readonly IAppDataLoader _loader;

        /// <summary>
        /// Creates scenes from data and manages them.
        /// </summary>
        private readonly IAppSceneManager _scenes;

        /// <summary>
        /// Pipe for all element updates.
        /// </summary>
        private readonly IElementTxnManager _txns;

        /// <inheritdoc />
        public string Id { get; private set; }

        /// <inheritdoc />
        public bool CanEdit { get; private set; }

        /// <inheritdoc />
        public bool CanDelete { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public AppController(
            IAppDataLoader loader,
            IAppSceneManager scenes,
            IElementTxnManager txns)
        {
            _loader = loader;
            _scenes = scenes;
            _txns = txns;
        }
        
        /// <inheritdoc />
        public IAsyncToken<Void> Load(string appId)
        {
            Id = appId;

            CanEdit = true;
            CanDelete = true;

            LogVerbose("Load().");

            return _loader.Load(appId);
        }

        /// <inheritdoc />
        public void Unload()
        {
            _loader.Unload();
            _scenes.Uninitialize();
            _txns.Uninitialize();
        }

        /// <inheritdoc />
        public void Play()
        {
            _scenes
                .Initialize(Id, _loader)
                .OnSuccess(_ =>
                {
                    _txns
                        .Initialize(Id, _scenes)
                        .OnSuccess(__ => Log.Info(this, "Txns initialized."))
                        .OnFailure(exception => Log.Error(this, "Could not initialize txns : {0}.", exception));
                })
                .OnFailure(exception =>
                {
                    Log.Error(this, "Could not initialize scenes : {0}.", exception);
                });
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