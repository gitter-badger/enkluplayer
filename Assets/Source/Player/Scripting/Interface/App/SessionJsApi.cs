using CreateAR.EnkluPlayer.Player.Session;
using Enklu.Orchid;

namespace CreateAR.EnkluPlayer.Scripting
{
    /// <summary>
    /// Session JS Interface for managing sessions.
    /// </summary>
    public class SessionJsApi
    {
        /// <summary>
        /// Session controller
        /// </summary>
        private readonly IPlayerSessionController _sessions;

        /// <summary>
        /// Creates a new <see cref="SessionJsApi"/> instance.
        /// </summary>
        public SessionJsApi(IPlayerSessionController sessions)
        {
            _sessions = sessions;
        }

        /// <summary>
        /// Scans mobile application QR, logs in and creates stargazer session
        /// </summary>
        public void start(IJsCallback callback)
        {
            if (null != _sessions.CurrentSession)
            {
                callback.Apply(this, "Session already active. End current session before starting a new session.", null);
                return;
            }

            _sessions
                .CreateSession()
                .OnSuccess(session =>
                {
                    if (null != session)
                    {
                        callback.Apply(this, "", session);
                    }
                    else
                    {
                        callback.Apply(this, "Failed to create session.", null);
                    }
                })
                .OnFailure(error =>
                {
                    callback.Apply(this, error.Message, null);
                });
        }

        /// <summary>
        /// Stops the current session
        /// </summary>
        public void stop()
        {
            if (null == _sessions.CurrentSession)
            {
                return;
            }

            _sessions.EndSession();
        }
    }
}