﻿using LightJson;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Txn.
    /// </summary>
    public class ElementTxnRequest
    {
        /// <summary>
        /// Id of the transaction.
        /// </summary>
        [JsonName("id")]
        public int Id;

        /// <summary>
        /// Actions.
        /// </summary>
        [JsonName("actions")]
        public ElementActionData[] Actions;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ElementTxnRequest(int id)
        {
            Id = id;
        }
    }

    /// <summary>
    /// Txn response.
    /// </summary>
    public class ElementTxnResponse
    {
        /// <summary>
        /// True iff txn was successful.
        /// </summary>
        [JsonName("success")]
        public bool Success;

        /// <summary>
        /// Error string, if any.
        /// </summary>
        [JsonName("error")]
        public string Error;
    }
}