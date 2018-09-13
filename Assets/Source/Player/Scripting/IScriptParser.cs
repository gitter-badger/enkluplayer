using CreateAR.EnkluPlayer.IUX;
using Jint.Parser;
using Jint.Parser.Ast;

namespace CreateAR.EnkluPlayer
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
        /// <param name="data">Data to hand to preprocessor.</param>
        /// <param name="options">Options.</param>
        /// <returns></returns>
        Program Parse(string code, ElementSchema data, ParserOptions options);
    }
}