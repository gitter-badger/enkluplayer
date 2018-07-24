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
    /// Preference data for a specific app.
    /// </summary>
    public class UserAppPreferenceData
    {
        /// <summary>
        /// Id of the app.
        /// </summary>
        public string AppId;

        /// <summary>
        /// Which mode to default to.
        /// </summary>
        public bool Play = true;
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

        /// <summary>
        /// Preference data for each app.
        /// </summary>
        public UserAppPreferenceData[] Apps = new UserAppPreferenceData[0];

        /// <summary>
        /// Retrieves preference data for an app. If preference data does not exist, it is created.
        /// </summary>
        /// <param name="appId">Id of the app.</param>
        /// <returns></returns>
        public UserAppPreferenceData App(string appId)
        {
            for (var i = 0; i < Apps.Length; i++)
            {
                var data = Apps[i];
                if (data.AppId == appId)
                {
                    return data;
                }
            }

            var appData = new UserAppPreferenceData
            {
                AppId = appId
            };
            Apps = Apps.Add(appData);

            return appData;
        }
    }
}