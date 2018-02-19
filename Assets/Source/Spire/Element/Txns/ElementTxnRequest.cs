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
}