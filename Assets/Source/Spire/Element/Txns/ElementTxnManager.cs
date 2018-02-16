using System.Collections.Generic;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Manages element transactions.
    /// </summary>
    public class ElementTxnManager : IElementTxnManager
    {
        /// <summary>
        /// Makes Http services.
        /// </summary>
        private readonly IHttpService _http;

        /// <summary>
        /// Lookup from sceneId -> scene loads.
        /// </summary>
        private readonly Dictionary<string, IAsyncToken<Void>> _sceneLoads = new Dictionary<string, IAsyncToken<Void>>();

        /// <summary>
        /// Lookup from sceneId -> store.
        /// </summary>
        private readonly Dictionary<string, IElementTxnStore> _stores = new Dictionary<string, IElementTxnStore>();

        /// <summary>
        /// Constructor.
        /// </summary>
        public ElementTxnManager(IHttpService http)
        {
            _http = http;
        }

        /// <inheritdoc />
        public IAsyncToken<Void> Initialize(string appId)
        {
            return new AsyncToken<Void>(Void.Instance);
        }

        /// <inheritdoc />
        public IAsyncToken<Void> Uninitialize()
        {
            return new AsyncToken<Void>(Void.Instance);
        }

        /// <inheritdoc />
        public void TrackScene(string sceneId)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public void UntrackScene(string sceneId)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public void Request(ElementTxn txn)
        {
            throw new System.NotImplementedException();
        }
    }
}