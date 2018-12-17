using System;
using System.Text;
using CreateAR.Commons.Unity.Logging;
using UnityEngine;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Saves logs to history.
    ///
    /// Note: Not thread safe!
    /// </summary>
    public class HistoryLogTarget : ILogTarget
    {
        [Flags]
        public enum LogDumpOptions
        {
            None = 0x0,
            Reverse = 0x1
        }

        /// <summary>
        /// Record of a log.
        /// </summary>
        private class LogRecord
        {
            /// <summary>
            /// The log level.
            /// </summary>
            public LogLevel Level;

            /// <summary>
            /// The log.
            /// </summary>
            public string FormattedLog;
        }

        /// <summary>
        /// Formats logs.
        /// </summary>
        private readonly ILogFormatter _formatter;

        /// <summary>
        /// How many logs to keep.
        /// </summary>
        private readonly int _size;

        /// <summary>
        /// Ring buffer of logs.
        /// </summary>
        private readonly LogRecord[] _logs;

        /// <summary>
        /// Index into log buffer.
        /// </summary>
        private int _index;

        /// <inheritdoc />
        public LogLevel Filter { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public HistoryLogTarget(
            ILogFormatter formatter,
            int maxLogs = 50)
        {
            _formatter = formatter;
            _size = maxLogs;
            _logs = new LogRecord[_size];

            for (var i = 0; i < _size; i++)
            {
                _logs[i] = new LogRecord();
            }
        }

        /// <inheritdoc />
        public void OnLog(LogLevel level, object caller, string message)
        {
            // keep all logs
            var record = _logs[_index % _size];
            record.Level = level;
            record.FormattedLog = _formatter.Format(level, caller, message);

            _index += 1;
        }

        /// <summary>
        /// Generates a dump of logs.
        /// </summary>
        /// <param name="length"></param>
        /// <param name="formatter"></param>
        /// <returns></returns>
        public string GenerateDump(LogDumpOptions options = LogDumpOptions.None)
        {
            // trivial case: no logs yet
            if (0 == _index)
            {
                return string.Empty;
            }

            var builder = new StringBuilder();

            for (var i = Mathf.Max(0, _index - 1); i > Mathf.Max(0, _index - _size + 1); i--)
            {
                var index = i % _size;
                var record = _logs[index];

                // filter here
                if (record.Level < Filter)
                {
                    continue;
                }

                // prepend
                if ((options & LogDumpOptions.Reverse) == 0)
                {
                    builder.Insert(0, record.FormattedLog + "\n");
                }
                // append
                else
                {
                    builder.AppendLine(record.FormattedLog);
                }
            }

            return builder.ToString();
        }
    }
}