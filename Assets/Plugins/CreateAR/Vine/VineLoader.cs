using System;
using System.IO;
using Antlr4.Runtime;
using CreateAR.Commons.Unity.Logging;
using UnityEngine;

namespace CreateAR.Commons.Vine
{
    public class VineLoader : IDisposable
    {
        private readonly TextWriter _stdOut;
        private readonly TextWriter _stdErr;

        public VineLoader()
        {
            try
            {
                _stdOut = File.CreateText(Path.Combine(
                    Application.persistentDataPath,
                    "Vine.stdout"));
            }
            catch
            {
                Log.Warning(this, "Could not get handle to Vine.stdout. Using /dev/null.");

                _stdOut = TextWriter.Null;
            }

            try
            {
                _stdErr = File.CreateText(Path.Combine(
                    Application.persistentDataPath,
                    "Vine.stderr"));
            }
            catch
            {
                Log.Warning(this, "Could not get handle to Vine.stderr. Using /dev/null.");

                _stdErr = TextWriter.Null;
            }
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