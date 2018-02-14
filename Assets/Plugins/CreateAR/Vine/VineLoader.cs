using System;
using System.IO;
using Antlr4.Runtime;
using UnityEngine;

namespace CreateAR.Commons.Vine
{
    public class VineLoader : IDisposable
    {
        private readonly TextWriter _stdOut;
        private readonly TextWriter _stdErr;

        public VineLoader()
        {
            _stdOut = File.CreateText(Path.Combine(
                Application.persistentDataPath,
                "Vine.stdout"));
            _stdErr = File.CreateText(Path.Combine(
                Application.persistentDataPath,
                "Vine.stderr"));
        }

        public VineParser.DocumentContext Load(string data)
        {
            var stream = new AntlrInputStream(data);
            var lexer = new VineLexer(stream, _stdOut, _stdErr);
            var tokenStream = new CommonTokenStream(lexer);
            var parser = new VineParser(tokenStream, _stdOut, _stdErr);
            
            return parser.document();
        }

        public void Dispose()
        {
            if (_stdOut != null)
            {
                _stdOut.Dispose();
            }

            if (_stdErr != null)
            {
                _stdErr.Dispose();
            }
        }
    }
}