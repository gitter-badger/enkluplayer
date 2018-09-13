using System;
using System.Text;
using CreateAR.Commons.Unity.Logging;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Log formatter for the conductor.
    /// </summary>
    public class ConductorLogFormatter : ILogFormatter
    {
        /// <inheritdoc />
        public string Format(LogLevel level, object caller, string message)
        {
            // send base64 encoded so that we know our divider (:) is not in any of the payloads
            return string.Format(
                "{0}:{1}:{2}:{3}:{4}",
                Convert.ToBase64String(Encoding.UTF8.GetBytes(level.ToString())),
                Convert.ToBase64String(Encoding.UTF8.GetBytes(DateTime.Now.ToString())),
                Convert.ToBase64String(Encoding.UTF8.GetBytes(null == caller ? "Null" : caller.GetType().FullName)),
                Convert.ToBase64String(Encoding.UTF8.GetBytes(null == caller ? "Null" : caller.ToString())),
                Convert.ToBase64String(Encoding.UTF8.GetBytes(message)));
        }
    }
}