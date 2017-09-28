using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Receives messages from a bridge.
    /// </summary>
    public class BridgeMessageHandler
    {
        /// <summary>
        /// Routes messages.
        /// </summary>
        private readonly IMessageRouter _router;
        
        /// <inheritdoc cref="IBridgeMessageHandler"/>
        public MessageTypeBinder Binder { get; private set; }
        
        /// <summary>
        /// Creates a new <c>BridgeMessageHandler</c>.
        /// </summary>
        public BridgeMessageHandler(IMessageRouter router)
        {
            _router = router;

            Binder = new MessageTypeBinder();
        }
        
        /// <summary>
        /// Called when a message is received.
        /// </summary>
        /// <param name="message">The message.</param>
        public void OnMessage(string message)
        {
            if (message.StartsWith("state;"))
            {
                message = message.Replace("state;", "");

                _router.Publish(MessageTypes.STATE, message);
                return;
            }

            Log.Debug(this, "Received [{0}]", message);

            var eventType = Binder.ByMessageType(message);
            if (-1 == eventType)
            {
                Log.Fatal(
                    this,
                    "Received a message for which we do not have a binding : {0}.",
                    message);
                return;
            }
            
            // publish
            _router.Publish(
                eventType,
                Void.Instance);
        }
    }
}