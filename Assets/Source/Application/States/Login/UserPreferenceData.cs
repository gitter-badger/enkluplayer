namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Data that associates the device and the org.
    /// </summary>
    public class DeviceRegistration
    {
        /// <summary>
        /// Id of the org.
        /// </summary>
        public string OrgId;

        /// <summary>
        /// Id of the device.
        /// </summary>
        public string DeviceId;
    }

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
        public DeviceRegistration[] DeviceRegistrations = new DeviceRegistration[0];

        /// <summary>
        /// If true, device registration has been ignored in the past.
        /// </summary>
        public bool IgnoreDeviceRegistration = false;
    }
}