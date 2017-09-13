namespace CreateAR.SpirePlayer
{
    public class UserCredentialsModel
    {
        public string token;
    }

    public class UserProfileModel
    {
        public string id;
        public string displayName;
    }

    /// <summary>
    /// Passed from the <c>IApplicationHost</c> to the <c>IApplicationHostDelegate</c>
    /// when we're authorized.
    /// </summary>
    public class AuthorizedEvent
    {
        public UserCredentialsModel credentials;
        public UserProfileModel profile;
    }
}