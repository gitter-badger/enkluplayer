namespace CreateAR.EnkluPlayer.Player.Session
{
    /// <summary>
    /// A data object containing information about the current player session.
    /// </summary>
    public class PlayerSession
    {
        /// <summary>
        /// The stargazer user identifier to associate with the session
        /// </summary>
        public string UserId { get; private set; }

        /// <summary>
        /// The session identifier.
        /// </summary>
        public string SessionId { get; private set; }

        /// <summary>
        /// Creates a new <see cref="PlayerSession"/> instance.
        /// </summary>
        public PlayerSession(string userId, string sessionId)
        {
            UserId = userId;
            SessionId = sessionId;
        }
    }
}