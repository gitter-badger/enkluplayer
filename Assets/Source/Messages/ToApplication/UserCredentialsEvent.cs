using Newtonsoft.Json;

namespace CreateAR.EnkluPlayer
{
    public class UserCredentialsModel
    {
        [JsonProperty("token")]
        public string Token;

        public override string ToString()
        {
            return string.Format("[UserCredentialsModel token={0}]",
                Token);
        }
    }

    public class UserProfileModel
    {
        [JsonProperty("id")]
        public string Id;
        
        [JsonProperty("displayName")]
        public string DisplayName;
        
        [JsonProperty("email")]
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
        [JsonProperty("credentials")]
        public UserCredentialsModel Credentials;
        
        [JsonProperty("profile")]
        public UserProfileModel Profile;

        public override string ToString()
        {
            return string.Format("[UserCredentialsEvent credentials={0}, profile={1}]",
                Credentials,
                Profile);
        }
    }
}