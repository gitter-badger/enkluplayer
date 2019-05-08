using System;
using Windows.System.Update;
using CreateAR.Commons.Unity.Logging;
using CreateAR.EnkluPlayer.Player.Session;
using Enklu.Orchid;

namespace CreateAR.EnkluPlayer.Scripting
{
    /// <summary>
    /// Session JS Interface for managing sessions.
    /// </summary>
    public class SessionJsApi : JsEventDispatcher
    {
        /// <summary>
        /// Session Data
        /// </summary>
        public class SessionDataJs
        {
            /// <summary>
            /// The user id which created the session.
            /// </summary>
            public string userId { get; private set; }

            /// <summary>
            /// The session id which created the session
            /// </summary>
            public string sessionId { get; private set; }

            /// <summary>
            /// Creates a new session data object
            /// </summary>
            internal SessionDataJs(PlayerSession session)
            {
                userId = session.UserId;
                sessionId = session.SessionId;
            }
        }

        /// <summary>
        /// Session controller
        /// </summary>
        private readonly IPlayerSessionController _sessions;

        /// <summary>
        /// The current session data.
        /// </summary>
        public SessionDataJs current { get; private set; }

        /// <summary>
        /// Dispatched when a session is created.
        /// </summary>
        private const string EVENT_SESSION_CREATED = "created";

        /// <summary>
        /// Dispatched when a session ends.
        /// </summary>
        private const string EVENT_SESSION_ENDED = "ended";

        /// <summary>
        /// Creates a new <see cref="SessionJsApi"/> instance.
        /// </summary>
        public SessionJsApi(IPlayerSessionController sessions)
        {
            _sessions = sessions;
            _sessions.OnSessionCreated += Sessions_OnSessionCreated;
            _sessions.OnSessionEnded += Sessions_OnSessionEnded;
        }
        
        /// <summary>
        /// Scans mobile application QR, logs in and creates stargazer session
        /// </summary>
        public void start(IJsCallback callback)
        {
            _sessions
                .CreateSession()
                .OnSuccess(session =>
                {
                    UpdateSession(session);

                    if (null != session)
                    {
                        callback.Apply(this, true, current);
                    }
                    else
                    {
                        callback.Apply(this, false);
                    }
                })
                .OnFailure(error =>
                {
                    callback.Apply(this, false);
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

        /// <summary>
        /// Handles session creation
        /// </summary>
        private void Sessions_OnSessionCreated(PlayerSession session)
        {
            UpdateSession(session);

            try
            {
                dispatch(EVENT_SESSION_CREATED, current);
            }
            catch (Exception exception)
            {
                Log.Error(this, "Could not dispatch session created event: {0}.", exception);
            }
        }

        /// <summary>
        /// Updates the session data 
        /// </summary>
        private void UpdateSession(PlayerSession session)
        {
            // Doesn't recreate if the data is the same
            if (null == current || current.userId != session.UserId || current.sessionId != session.SessionId)
            {
                current = new SessionDataJs(session);
            }
        }

        /// <summary>
        /// Handles session ending.
        /// </summary>
        private void Sessions_OnSessionEnded()
        {
            current = null;

            try
            {
                dispatch(EVENT_SESSION_ENDED);
            }
            catch (Exception exception)
            {
                Log.Error(this, "Could not dispatch session ended event: {0}.", exception);
            }
        }
    }
}