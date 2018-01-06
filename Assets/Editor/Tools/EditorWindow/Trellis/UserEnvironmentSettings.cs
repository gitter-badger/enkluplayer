namespace CreateAR.SpirePlayer.Editor
{
    /// <summary>
    /// Contains credentials for all environments.
    /// </summary>
    public class UserEnvironmentSettings
    {
        /// <summary>
        /// All credentials for all environments.
        /// </summary>
        public EnvironmentCredentials[] All = new EnvironmentCredentials[0];

        /// <summary>
        /// Selected environment.
        /// </summary>
        public string Environment;

        /// <summary>
        /// Retrieves credentials for an environment.
        /// </summary>
        /// <param name="environment">The name of the environment in question.</param>
        /// <returns></returns>
        public EnvironmentCredentials Credentials(string environment)
        {
            if (null == All)
            {
                return null;
            }

            for (int i = 0, len = All.Length; i < len; i++)
            {
                var creds = All[i];
                if (creds.Environment == environment)
                {
                    return creds;
                }
            }

            return null;
        }

        /// <summary>
        /// Useful ToString override.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format(
                "[EnvironmentCredentialsConfig Length={0}]",
                null == All ? 0 : All.Length);
        }
    }
}