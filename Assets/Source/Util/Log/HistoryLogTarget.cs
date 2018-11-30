using System;
using System.Text;
using CreateAR.Commons.Unity.Logging;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Saves logs to history.
    ///
    /// Note: Not thread safe!
    /// </summary>
    public class HistoryLogTarget : ILogTarget
    {
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
        private readonly string[] _logs;

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
            _logs = new string[_size];
        }

        /// <inheritdoc />
        public void OnLog(LogLevel level, object caller, string message)
        {
            if (level < Filter)
            {
                return;
            }

            _logs[_index % _size] = _formatter.Format(level, caller, message);
            _index += 1;
        }

        /// <summary>
        /// Generates a dump of logs.
        /// </summary>
        /// <returns></returns>
        public string GenerateDump()
        {
            // trivial case: no logs yet
            if (0 == _index)
            {
                return string.Empty;
            }

            var builder = new StringBuilder();

            var start = Math.Max(0, _index - _size);
            var end = _index;
            for (var i = start; i <= end; i++)
            {
                var index = i % _size;

                builder.AppendLine(_logs[index]);
            }

            return builder.ToString();
        }
    }
}