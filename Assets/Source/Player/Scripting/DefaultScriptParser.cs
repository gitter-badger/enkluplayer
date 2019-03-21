using CreateAR.Commons.Unity.Logging;
using CreateAR.EnkluPlayer.IUX;
using CreateAR.EnkluPlayer.Vine;
using Jint.Parser;
using Jint.Parser.Ast;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Default implementation simple parses synchronously.
    /// </summary>
    public class DefaultScriptParser : IScriptParser
    {
        /// <summary>
        /// Tracking metrics.
        /// </summary>
        private readonly IMetricsService _metrics;

        /// <summary>
        /// Preprocesses.
        /// </summary>
        private readonly IVinePreProcessor _preprocessor;

        /// <summary>
        /// Constructor.
        /// </summary>
        public DefaultScriptParser(
            IMetricsService metrics,
            IVinePreProcessor preprocessor)
        {
            _metrics = metrics;
            _preprocessor = preprocessor;
        }

        /// <inheritdoc cref="IScriptParser"/>
        public string Parse(string code, ElementSchema data)
        {
            var id = _metrics.Timer(MetricsKeys.SCRIPT_PARSING_BEHAVIOR).Start();

            _preprocessor.DataStore = data;

            var processed = _preprocessor.Execute(code);

            Log.Info(this, "Parse : {0}.", processed);

            _metrics.Timer(MetricsKeys.SCRIPT_PARSING_BEHAVIOR).Stop(id);

            return processed;
        }
    }
}