using System.IO;
using Antlr4.Runtime;
using UnityEngine;

namespace CreateAR.Commons.Vine
{
    /// <summary>
    /// Loads a Vine document from a string of data.
    /// </summary>
    public class VineLoader
    {
        /// <summary>
        /// StdOut.
        /// </summary>
        private static readonly StreamWriter _out = new StreamWriter(File.OpenWrite(Path.Combine(
            Application.persistentDataPath,
            "Parser.std.out")));

        /// <summary>
        /// StdErr.
        /// </summary>
        private static readonly StreamWriter _err = new StreamWriter(File.OpenWrite(Path.Combine(
            Application.persistentDataPath,
            "Parser.error.out")));

        /// <summary>
        /// Loads a Vine document from a string.
        /// </summary>
        /// <param name="data">The Vine source.</param>
        /// <returns></returns>
        public VineParser.DocumentContext Load(string data)
        {
            var stream = new AntlrInputStream(data);
            var lexer = new VineLexer(stream, _out, _err);
            var tokenStream = new CommonTokenStream(lexer);
            var parser = new VineParser(tokenStream, _out, _err);
            
            return parser.document();
        }
    }
}