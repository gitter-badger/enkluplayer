using CreateAR.Commons.Unity.Http;

namespace CreateAR.EnkluPlayer.Player.Session
{

    public class StargazerCredentials
    {
        /// <summary>
        /// User id.
        /// </summary>
        public string UserId;

        /// <summary>
        /// Token!
        /// </summary>
        public string Token;
        
        /// <summary>
        /// Useful ToString override.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format(
                "[StargazerCredentials UserId={0}, Token={1}]",
                UserId,
                Token);
        }

        /// <summary>
        /// Applies credentials to HTTP service.
        /// </summary>
        /// <param name="http">Makes http calls.</param>
        public void Apply(IHttpService http)
        {
            http.Services.Urls.Formatter("stargazer").Replacements["userId"] = UserId;
            http.Services.AddHeader("stargazer", "Authorization", string.Format("Bearer {0}", Token));
        }
    }
}