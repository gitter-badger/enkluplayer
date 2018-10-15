using Newtonsoft.Json;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Txn.
    /// </summary>
    public class ElementTxnRequest
    {
        /// <summary>
        /// Id of the transaction.
        /// </summary>
        [JsonProperty("id")]
        public int Id;

        /// <summary>
        /// Actions.
        /// </summary>
        [JsonProperty("actions")]
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
        [JsonProperty("success")]
        public bool Success;

        /// <summary>
        /// Error string, if any.
        /// </summary>
        [JsonProperty("error")]
        public string Error;
    }
}