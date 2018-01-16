using System;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Application wide configuration.
    /// </summary>
    [Serializable]
    public class ApplicationConfig
    {
        /// <summary>
        /// Network configuration.
        /// </summary>
        public ApplicationNetworkConfig Network;

        /// <summary>
        /// ToString override.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format(
                "[ApplicationConfig Network={0}]",
                Network);
        }
    }

    /// <summary>
    /// Network related configuration.
    /// </summary>
    public class ApplicationNetworkConfig
    {
        /// <summary>
        /// Complete Trellis url.
        /// </summary>
        public string TrellisUrl;

        /// <summary>
        /// True iff the player should automatically login.
        /// </summary>
        public bool AutoLogin;

        /// <summary>
        /// The user id to login with.
        /// </summary>
        public string AutoLoginUserId;

        /// <summary>
        /// The token to login with.
        /// </summary>
        public string AutoLoginToken;

        /// <summary>
        /// Useful ToString..
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format(
                "[AutoLogin={0}, UserId={1}, Token={2}]",
                AutoLogin,
                AutoLoginUserId,
                AutoLoginToken);
        }
    }
}