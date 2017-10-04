﻿using CreateAR.Commons.Unity.Logging;

namespace CreateAR.Spire
{
    /// <summary>
    /// Wraps logging interface for Js.
    /// </summary>
    public class JsLogWrapper
    {
        private readonly object _context;

        /// <summary>
        /// Creates a new JsLogWrapper.
        /// </summary>
        /// <param name="context">The context.</param>
        public JsLogWrapper(object context)
        {
            _context = context;
        }

        /// <summary>
        /// Debug level logging.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public void debug(object message, params object[] replacements)
        {
            Log.Debug(_context, string.Format(message.ToString(), replacements));
        }

        /// <summary>
        /// Info level logging.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public void info(object message, params object[] replacements)
        {
            Log.Info(_context, string.Format(message.ToString(), replacements));
        }

        /// <summary>
        /// Warn level logging.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public void warn(object message, params object[] replacements)
        {
            Log.Warning(_context, string.Format(message.ToString(), replacements));
        }

        /// <summary>
        /// Error level logging.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public void error(object message, params object[] replacements)
        {
            Log.Error(_context, string.Format(message.ToString(), replacements));
        }
    }
}
