using Antlr4.Runtime;

namespace CreateAR.Commons.Vine
{
    public class VineLoader
    {
        public VineParser.DocumentContext Load(string data)
        {
            var stream = new AntlrInputStream(data);
            var lexer = new VineLexer(stream);
            var tokenStream = new CommonTokenStream(lexer);
            var parser = new VineParser(tokenStream);
            
            return parser.document();
        }
    }
}