using LightJson;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Txn.
    /// </summary>
    public class ElementTxnRequest
    {
        /// <summary>
        /// Actions.
        /// </summary>
        [JsonName("actions")]
        public ElementRequest[] Actions;
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