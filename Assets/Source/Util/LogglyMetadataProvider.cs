using System.Collections.Generic;
using CreateAR.Commons.Unity.Messaging;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Provides user metadata to logglu.
    /// </summary>
    public class LogglyMetadataProvider : ILogglyMetadataProvider
    {
        /// <summary>
        /// Meta information.
        /// </summary>
        public Dictionary<string, string> Meta { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="messages">Dispatches messages.</param>
        /// <param name="config">Application configuration.</param>
        public LogglyMetadataProvider(
            IMessageRouter messages,
            ApplicationConfig config)
        {
            Meta = new Dictionary<string, string>();

            messages.Subscribe(
                MessageTypes.PLAY,
                _ =>
                {
                    var profile = config.Network.Credentials(
                        config.Network.Current);

                    Meta["userId"] = profile.UserId;
                    Meta["email"] = profile.Email;
                    Meta["environment"] = profile.Environment;
                });
        }
    }
}