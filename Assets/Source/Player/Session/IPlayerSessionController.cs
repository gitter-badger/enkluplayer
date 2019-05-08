using System;
using CreateAR.Commons.Unity.Async;

namespace CreateAR.EnkluPlayer.Player.Session
{
    /// <summary>
    /// Implementation prototype for an object capable of managing sessions.
    /// </summary>
    public interface IPlayerSessionController
    {
        /// <summary>
        /// Fired whenever the current session data has changed.
        /// </summary>
        event Action<PlayerSession> OnSessionCreated;

        /// <summary>
        /// Fired whenever the current session has ended.
        /// </summary>
        event Action OnSessionEnded;

        /// <summary>
        /// The current player session.
        /// </summary>
        PlayerSession CurrentSession { get; }

        /// <summary>
        /// Creates a new session.
        /// </summary>
        IAsyncToken<PlayerSession> CreateSession();

        /// <summary>
        /// Ends the current session.
        /// </summary>
        void EndSession();
    }
}