using System.Collections.Generic;
using System.Diagnostics;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Outpus log of Url transformation.
    /// </summary>
    public class LoggedUrlFormatter : UrlFormatter
    {
        /// <inheritdoc />
        public override string Url(string endpoint, string version, int port, string protocol, Dictionary<string, string> replacements = null)
        {
            var newUrl = base.Url(endpoint, version, port, protocol, replacements);

            Verbose("UrlFormatter({0}) -> {1}",
                endpoint,
                newUrl);

            return newUrl;
        }

        /// <summary>
        /// Logs.
        /// </summary>
        [Conditional("LOGGING_VERBOSE")]
        private void Verbose(string message, params object[] replacements)
        {
            Log.Info(this, message, replacements);
        }
    }
}