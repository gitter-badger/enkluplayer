using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Messaging;
using CreateAR.Spire;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Bridge that pushes straight to play a specific scene.
    /// </summary>
    public class ReleaseBridge : IBridge
    {
        /// <summary>
        /// Messages.
        /// </summary>
        private readonly IMessageRouter _messages;

        /// <inheritdoc cref="IBridge"/>
        public MessageTypeBinder Binder { get; private set; }

        /// <summary>
        /// Creates a new bridge for release mode.
        /// </summary>
        /// <param name="messages">Messages.</param>
        public ReleaseBridge(IMessageRouter messages)
        {
            _messages = messages;

            Binder = new MessageTypeBinder();
        }

        /// <inheritdoc cref="IBridge"/>
        public void BroadcastReady()
        {
            // play immediately
            _messages.Publish(
                MessageTypes.PLAY,
                Void.Instance);
        }
    }
}