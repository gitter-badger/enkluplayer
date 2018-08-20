using CreateAR.Commons.Unity.Logging;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Wraps logging interface for Js.
    /// </summary>
    public class LogJsApi
    {   
        /// <summary>
        /// Context to pass to each method.
        /// </summary>
        private readonly object _context;

        /// <summary>
        /// Creates a new JsLogWrapper.
        /// </summary>
        /// <param name="context">The context.</param>
        public LogJsApi(object context)
        {
            _context = context;
        }

        /// <summary>
        /// Debug level logging.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public void debug(object message, params object[] replacements)
        {
            if (null == message)
            {
                Log.Debug(_context, "null");
                return;
            }

            Log.Debug(_context, string.Format(message.ToString(), replacements));
        }

        /// <summary>
        /// Info level logging.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public void info(object message, params object[] replacements)
        {
            if (null == message)
            {
                Log.Info(_context, "null");
                return;
            }

            Log.Info(_context, string.Format(message.ToString(), replacements));
        }

        /// <summary>
        /// Warn level logging.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public void warn(object message, params object[] replacements)
        {
            if (null == message)
            {
                Log.Warning(_context, "null");
                return;
            }

            Log.Warning(_context, string.Format(message.ToString(), replacements));
        }

        /// <summary>
        /// Error level logging.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public void error(object message, params object[] replacements)
        {
            if (null == message)
            {
                Log.Error(_context, "null");
                return;
            }

            Log.Error(_context, string.Format(message.ToString(), replacements));
        }
    }
}
