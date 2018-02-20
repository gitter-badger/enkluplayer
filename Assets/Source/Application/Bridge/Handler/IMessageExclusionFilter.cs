namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Describes an object that can filter messages.
    /// </summary>
    public interface IMessageExclusionFilter
    {
        /// <summary>
        /// True iff the filter wants to exclude the message.
        /// </summary>
        /// <param name="messageType">The message type.</param>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        bool Exclude(int messageType, object message);
    }

    public class ElementUpdateFilter : IMessageExclusionFilter
    {
        private readonly string _userId;

        public ElementUpdateFilter(string userId)
        {

        }

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