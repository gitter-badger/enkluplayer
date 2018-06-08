using CreateAR.Commons.Unity.Async;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Describes an interface for an object that listens for scene changes as
    /// well as requests scene changes from server.
    /// </summary>
    public interface IElementTxnManager
    {
        /// <summary>
        /// Initializes the manager for an app.
        /// </summary>
        /// <param name="configuration">Contains values for how to configure the manager.</param>
        /// <returns></returns>
        IAsyncToken<Void> Initialize(AppTxnConfiguration configuration);

        /// <summary>
        /// Unintializes the manager.
        /// </summary>
        /// <returns></returns>
        IAsyncToken<Void> Uninitialize();
        
        /// <summary>
        /// Requests a scene change.
        /// </summary>
        /// <param name="txn">The transaction we're requesting.</param>
        IAsyncToken<ElementResponse> Request(ElementTxn txn);

        /// <summary>
        /// Applies a scene change.
        /// </summary>
        /// <param name="txn">The transaction we're requesting.</param>
        ElementResponse Apply(ElementTxn txn);

        /// <summary>
        /// Determines whether or not a txn originated here.
        /// </summary>
        /// <param name="txnId">Id of the transaction.</param>
        /// <returns></returns>
        bool IsTracked(long txnId);
    }
}