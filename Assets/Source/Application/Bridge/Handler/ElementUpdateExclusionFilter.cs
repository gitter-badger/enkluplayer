namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Excludes element updates from a specific user.
    /// </summary>
    public class ElementUpdateExclusionFilter : IMessageExclusionFilter
    {
        /// <summary>
        /// Userid.
        /// </summary>
        private readonly string _userId;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ElementUpdateExclusionFilter(string userId)
        {
            _userId = userId;
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
                    return ((SceneEvent) message).User == _userId;
                }
                default:
                {
                    return false;
                }
            }
        }
    }
}