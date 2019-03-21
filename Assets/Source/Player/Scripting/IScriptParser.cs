using CreateAR.EnkluPlayer.IUX;

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
        /// <returns></returns>
        string Parse(string code, ElementSchema data);
    }
}