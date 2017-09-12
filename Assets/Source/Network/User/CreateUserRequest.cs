namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Request for creating a user.
    /// </summary>
    public class CreateUserRequest
    {
        public string displayName;
        public string provider;
        public string providerToken;
    }
}