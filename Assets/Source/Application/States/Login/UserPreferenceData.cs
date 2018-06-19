namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Preference data for a user.
    /// </summary>
    public class UserPreferenceData
    {
        /// <summary>
        /// Id of the user.
        /// </summary>
        public string UserId;

        /// <summary>
        /// App id of the most recently accessed app.
        /// </summary>
        public string MostRecentAppId;

        /// <summary>
        /// All organizations this device is registered to.
        /// </summary>
        public string[] Orgs = new string[0];

        /// <summary>
        /// If true, device registration has been ignored in the past.
        /// </summary>
        public bool IgnoreDeviceRegistration = false;
    }
}