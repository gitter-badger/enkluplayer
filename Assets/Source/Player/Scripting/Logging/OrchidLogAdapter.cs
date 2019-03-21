using Enklu.Orchid.Logging;
using Log = CreateAR.Commons.Unity.Logging.Log;

namespace CreateAR.EnkluPlayer.Scripting.Logging {

    /// <summary>
    /// Logging adapter to ensure Orchid libraries output using our logging library.
    /// </summary>
    public class OrchidLogAdapter : ILogAdapter
    {
        /// <inheritdoc/>
        public void Debug(object caller, object message, params object[] replacements)
        {
            Log.Debug(caller, message, replacements);
        }

        /// <inheritdoc/>
        public void Info(object caller, object message, params object[] replacements)
        {
            Log.Info(caller, message, replacements);
        }

        /// <inheritdoc/>
        public void Warning(object caller, object message, params object[] replacements)
        {
            Log.Warning(caller, message, replacements);
        }

        /// <inheritdoc/>
        public void Error(object caller, object message, params object[] replacements)
        {
            Log.Error(caller, message, replacements);
        }

        /// <inheritdoc/>
        public void Fatal(object caller, object message, params object[] replacements)
        {
            Log.Fatal(caller, message, replacements);
        }
    }
}