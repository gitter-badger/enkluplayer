namespace CreateAR.SpirePlayer
{
    public class UserCredentialsModel
    {
        public string token;

        public override string ToString()
        {
            return string.Format("[UserCredentialsModel token={0}]",
                token);
        }
    }

    public class UserProfileModel
    {
        public string id;
        public string displayName;
        public string email;

        public override string ToString()
        {
            return string.Format("[UserProfileModel id={0}, displayName={1}, email={2}]",
                id,
                displayName,
                email);
        }
    }
    
    /// <summary>
    /// Passed from the <c>IApplicationHost</c> to the <c>IApplicationHostDelegate</c>
    /// when we're authorized.
    /// </summary>
    public class AuthorizedEvent
    {
        public UserCredentialsModel credentials;
        public UserProfileModel profile;

        public override string ToString()
        {
            return string.Format("[AuthorizedEvent credentials={0}, profile={1}]",
                credentials,
                profile);
        }
    }
}