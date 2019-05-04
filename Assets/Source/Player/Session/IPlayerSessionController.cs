using CreateAR.Commons.Unity.Async;

namespace CreateAR.EnkluPlayer.Player.Session
{
    /// <summary>
    /// Implementation prototype for an object capable of managing sessions.
    /// </summary>
    public interface IPlayerSessionController
    {
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