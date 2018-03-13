namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Excludes element updates from a specific user.
    /// </summary>
    public class ElementUpdateExclusionFilter : IMessageExclusionFilter
    {
        /// <summary>
        /// Txn manager.
        /// </summary>
        private readonly IElementTxnManager _txns;
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public ElementUpdateExclusionFilter(IElementTxnManager txns)
        {
            _txns = txns;
        }

        /// <inheritdoc />
        public bool Exclude(int messageType, object message)
        {
            switch (messageType)
            {
                case MessageTypes.SCENE_CREATE:
                case MessageTypes.SCENE_UPDATE:
                case MessageTypes.SCENE_DELETE:
                {
                    return _txns.IsTracked(((SceneEvent) message).Id);
                }
                default:
                {
                    return false;
                }
            }
        }
    }
}