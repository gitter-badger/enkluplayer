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

            try
            {
                return _parser.Parse(processed, options);
            }
            catch (ParserException exception)
            {
                Log.Warning(this, "Could not parse JS program : {0}.", exception);

                return null;
            }
        }
    }
}