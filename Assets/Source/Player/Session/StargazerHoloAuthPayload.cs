namespace CreateAR.EnkluPlayer.Player.Session
{
    /// <summary>
    /// Stargazer hololens auth payload.
    /// </summary>
    public class StargazerHoloAuthPayload
    {
        /// <summary>
        /// The user id to login to stargazer.
        /// </summary>
        public string UserId { get; private set; }

        /// <summary>
        /// The code used to login to stargazer and receive an auth token.
        /// </summary>
        public string Code { get; private set; }

        /// <summary>
        /// Creates a new <see cref="StargazerHoloAuthPayload"/> instance.
        /// </summary>
        public StargazerHoloAuthPayload(string userId, string code)
        {
            UserId = userId;
            Code = code;
        }
    }
}