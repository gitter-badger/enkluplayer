using CreateAR.Commons.Unity.Logging;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// <c>IApplicationHost</c> implementation for webpages.
    /// </summary>
    public class WebApplicationHost : IApplicationHost
    {
        /// <inheritdoc cref="IApplicationHost"/>
        public void Ready(IApplicationHostDelegate @delegate)
        {
            Log.Info(this, "Application is ready.");

            throw new System.NotImplementedException();
        }
    }
}