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
        /// Loads a Vine document from a string.
        /// </summary>
        /// <param name="data">The Vine source.</param>
        /// <returns></returns>
        public VineParser.DocumentContext Load(string data)
        {
            var stdout = new StreamWriter(File.OpenWrite(Path.Combine(
                Application.persistentDataPath,
                "Parser.std.out")));
            var err = new StreamWriter(File.OpenWrite(Path.Combine(
                Application.persistentDataPath,
                "Parser.error.out")));

            var stream = new AntlrInputStream(data);
            var lexer = new VineLexer(stream, stdout, err);
            var tokenStream = new CommonTokenStream(lexer);
            var parser = new VineParser(tokenStream, stdout, err);
            
            return parser.document();
        }
    }
}