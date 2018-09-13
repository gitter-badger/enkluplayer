using LightJson;

namespace CreateAR.EnkluPlayer
{
    public class UserCredentialsModel
    {
        [JsonName("token")]
        public string Token;

        public override string ToString()
        {
            return string.Format("[UserCredentialsModel token={0}]",
                Token);
        }
    }

    public class UserProfileModel
    {
        [JsonName("id")]
        public string Id;
        
        [JsonName("displayName")]
        public string DisplayName;
        
        [JsonName("email")]
        public string Email;

        public override string ToString()
        {
            return string.Format("[UserProfileModel id={0}, displayName={1}, email={2}]",
                Id,
                DisplayName,
                Email);
        }
    }
    
    /// <summary>
    /// Authorization information.
    /// </summary>
    public class UserCredentialsEvent
    {
        [JsonName("credentials")]
        public UserCredentialsModel Credentials;
        
        [JsonName("profile")]
        public UserProfileModel Profile;

        public override string ToString()
        {
            return string.Format("[UserCredentialsEvent credentials={0}, profile={1}]",
                Credentials,
                Profile);
        }
    }
}