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
        /// Retrieves an environment by name.
        /// </summary>
        /// <param name="name">Environment name.</param>
        /// <returns></returns>
        public EnvironmentData Environment(string name)
        {
            if (null == Environments)
            {
                return null;
            }

            for (int i = 0, len = Environments.Length; i < len; i++)
            {
                var env = Environments[i];
                if (env.Name == name)
                {
                    return env;
                }
            }

            return null;
        }

        /// <summary>
        /// Useful ToString.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format(
                "[EnvironmentConfig Count={0}]",
                null == Environments ? 0 : Environments.Length);
        }
    }
}