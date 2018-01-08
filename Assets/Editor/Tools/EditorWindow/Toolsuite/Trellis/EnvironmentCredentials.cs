namespace CreateAR.SpirePlayer.Editor
{
    /// <summary>
    /// Simple POCO for editor interaction with Trellis.
    /// </summary>
    public class EnvironmentCredentials
    {
        /// <summary>
        /// Environment these credentials are for.
        /// </summary>
        public string Environment;
        
        /// <summary>
        /// Email.
        /// </summary>
        public string Email;
        
        /// <summary>
        /// Password.
        /// </summary>
        public string Password;
        
        /// <summary>
        /// Token!
        /// </summary>
        public string Token;

        /// <summary>
        /// Useful ToString override.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format(
                "[TrellisCredentials Email={0}, Password={1}, Token={2}]",
                Email,
                Password,
                Token);
        }
    }
}