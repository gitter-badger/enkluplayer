namespace CreateAR.SpirePlayer.Editor
{
    /// <summary>
    /// Holds information about environments.
    /// </summary>
    public class EnvironmentConfig
    {
        /// <summary>
        /// List of environments.
        /// </summary>
        public EnvironmentData[] Environments;

        /// <summary>
        /// Name of the selected environments.
        /// </summary>
        public string SelectedEnvironment;

        /// <summary>
        /// Retrieves the currently selected environment.
        /// </summary>
        public EnvironmentData Selected
        {
            get
            {
                for (int i = 0, len = Environments.Length; i < len; i++)
                {
                    var env = Environments[i];
                    if (env.Name == SelectedEnvironment)
                    {
                        return env;
                    }
                }

                if (0 == Environments.Length)
                {
                    return null;
                }

                return Environments[0];
            }
        }
    }
}