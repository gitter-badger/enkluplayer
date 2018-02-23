namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Describes an object that applies actions, but is able to commit them or
    /// roll them back later.
    /// </summary>
    public interface IElementTxnStore
    {
        /// <summary>
        /// Requests that a transaction take place. The store decides whether
        /// to precommit and rollback on failure, or to wait for commit.
        /// 
        /// If there is an error on application, the transaction is
        /// automatically rolled back and the error parameter will be filled.
        /// </summary>
        /// <param name="txn">The transaction.</param>
        /// <param name="error">Error, if any.</param>
        /// <returns>True iff the txn was applied.</returns>
        bool Request(ElementTxn txn, out string error);
        
        /// <summary>
        /// Used in conjunction with Apply. This tells the store it can forget
        /// about the txns state. Rollback cannot be called later.
        /// </summary>
        /// <param name="id">Unique id of txn.</param>
        void Commit(uint id);

        /// <summary>
        /// Used in conjunction with Apply. Tells the store to apply an undo
        /// operation for a txn. Commit cannot be called later.
        /// </summary>
        /// <param name="id">Unique id of txn.</param>
        void Rollback(uint id);

        /// <summary>
        /// Applies a transaction without keeping a record of past state. This
        /// is used for txns we already know should be committed, regardless
        /// of whether the actions fail or succeed.
        /// </summary>
        /// <param name="txn">The transaction.</param>
        void Apply(ElementTxn txn);
    }
}