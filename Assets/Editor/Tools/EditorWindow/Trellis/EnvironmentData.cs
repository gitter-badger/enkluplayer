namespace CreateAR.SpirePlayer.Editor
{
    /// <summary>
    /// Holds information about a specific environment.
    /// </summary>
    public class EnvironmentData
    {
        /// <summary>
        /// Name of this environment.
        /// </summary>
        public string Name = "local";
        
        /// <summary>
        /// Hostname.
        /// </summary>
        public string Hostname = "localhost";
        
        /// <summary>
        /// Port.
        /// </summary>
        public int Port = 9999;
        
        /// <summary>
        /// Api version.
        /// </summary>
        public string ApiVersion = "v1";
    }
}