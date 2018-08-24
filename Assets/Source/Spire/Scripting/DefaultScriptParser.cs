using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.IUX;
using CreateAR.SpirePlayer.Vine;
using Jint.Parser;
using Jint.Parser.Ast;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Default implementation simple parses synchronously.
    /// </summary>
    public class DefaultScriptParser : IScriptParser
    {
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
            IVinePreProcessor preprocessor,
            JavaScriptParser parser)
        {
            _preprocessor = preprocessor;
            _parser = parser;
        }
        
        /// <inheritdoc cref="IScriptParser"/>
        public Program Parse(string code, ElementSchema data, ParserOptions options)
        {
            _preprocessor.DataStore = data;

            var processed = _preprocessor.Execute(code);

            Log.Info(this, "Parse : {0}.", processed);

            return _parser.Parse(processed, options);
        }
    }
}