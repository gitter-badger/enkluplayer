using CreateAR.Commons.Unity.Async;
using Jint.Parser;
using Jint.Parser.Ast;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Interface for parsing.
    /// </summary>
    public interface IScriptParser
    {
        /// <summary>
        /// Parses code into a <c>Program</c>.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <returns></returns>
        IAsyncToken<Program> Parse(string code);

        /// <summary>
        /// Parses code into a <c>Program</c>.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="options">Options.</param>
        /// <returns></returns>
        IAsyncToken<Program> Parse(string code, ParserOptions options);
    }
}