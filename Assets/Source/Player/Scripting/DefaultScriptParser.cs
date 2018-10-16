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
        /// Jint implementation.
        /// </summary>
        private readonly JavaScriptParser _parser;

        /// <summary>
        /// Constructor.
        /// </summary>
        public DefaultScriptParser(
            IMetricsService metrics,
            IVinePreProcessor preprocessor,
            JavaScriptParser parser)
        {
            _metrics = metrics;
            _preprocessor = preprocessor;
            _parser = parser;
        }
        
        /// <inheritdoc cref="IScriptParser"/>
        public Program Parse(string code, ElementSchema data, ParserOptions options)
        {
            var id = _metrics.Timer(MetricsKeys.SCRIPT_PARSING_BEHAVIOR).Start();

            _preprocessor.DataStore = data;

            var processed = _preprocessor.Execute(code);

            Log.Info(this, "Parse : {0}.", processed);

            try
            {
                var program = _parser.Parse(processed, options);

                _metrics.Timer(MetricsKeys.SCRIPT_PARSING_BEHAVIOR).Stop(id);

                return program;
            }
            catch (ParserException exception)
            {
                Log.Warning(this, "Could not parse JS program : {0}.", exception);

                _metrics.Timer(MetricsKeys.SCRIPT_PARSING_BEHAVIOR).Abort(id);

                return null;
            }
        }
    }
}