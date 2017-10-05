using CreateAR.Commons.Unity.Async;
using Jint.Parser;
using Jint.Parser.Ast;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Default implementation simple parses synchronously.
    /// 
    /// TODO: Queue + parse in separate worker thread or coroutine (webgl).
    /// </summary>
    public class DefaultScriptParser : IScriptParser
    {
        /// <summary>
        /// Jint implementation.
        /// </summary>
        private readonly JavaScriptParser _parser;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="parser">Jint parser.</param>
        public DefaultScriptParser(JavaScriptParser parser)
        {
            _parser = parser;
        }

        /// <inheritdoc cref="IScriptParser"/>
        public IAsyncToken<Program> Parse(string code)
        {
            return new AsyncToken<Program>(_parser.Parse(code));
        }

        /// <inheritdoc cref="IScriptParser"/>
        public IAsyncToken<Program> Parse(string code, ParserOptions options)
        {
            return new AsyncToken<Program>(_parser.Parse(code, options));
        }
    }
}