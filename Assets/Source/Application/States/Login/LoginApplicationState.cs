using System;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Decides how to login.
    /// </summary>
    public class LoginApplicationState : IState
    {
        /// <summary>
        /// Credentials.
        /// </summary>
        private const string CREDS = "login.creds";

        /// <summary>
        /// Caches bytes on disk.
        /// </summary>
        private readonly IDiskCache _cache;

        /// <summary>
        /// Pub/sub interface.
        /// </summary>
        private readonly IMessageRouter _messages;

        /// <summary>
        /// Serializer.
        /// </summary>
        private readonly ISerializer _serializer;

        /// <summary>
        /// Strategy for logging in.
        /// </summary>
        private readonly ILoginStrategy _strategy;

        /// <summary>
        /// Constructor.
        /// </summary>
        public LoginApplicationState(
            IDiskCache cache,
            IMessageRouter messages,
            ISerializer serializer,
            ILoginStrategy strategy)
        {
            _cache = cache;
            _messages = messages;
            _serializer = serializer;
            _strategy = strategy;
        }

        /// <inheritdoc />
        public void Enter(object context)
        {
            // check disk cache for credentials
            if (_cache.Contains(CREDS))
            {
                _cache
                    .Load(CREDS)
                    .OnSuccess(bytes =>
                    {
                        var data = bytes;
                        object obj;

                        try
                        {
                            _serializer.Deserialize(
                                typeof(CredentialsData),
                                ref data,
                                out obj);
                        }
                        catch (Exception exception)
                        {
                            Log.Error(this, "Could not deserialize saved credentials: {0}", exception);
                        }

                        // load into default app

                    })
                    .OnFailure(exception =>
                    {
                        Log.Error(this, "Could not load credential information: {0}", exception);

                        // login
                    });
            }
            else
            {
                // login
            }
        }

        /// <inheritdoc />
        public void Update(float dt)
        {
            
        }

        /// <inheritdoc />
        public void Exit()
        {
            
        }
    }
}