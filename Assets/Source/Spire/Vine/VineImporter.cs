using Antlr4.Runtime.Tree;
using CreateAR.Commons.Vine;
using CreateAR.SpirePlayer.IUX;

namespace CreateAR.SpirePlayer.Vine
{
    /// <summary>
    /// Creates an <c>ElementDescription</c> from data.
    /// </summary>
    public class VineImporter
    {
        /// <summary>
        /// Parses the data.
        /// </summary>
        /// <param name="data">String data.</param>
        /// <returns></returns>
        public ElementDescription Parse(string data)
        {
            var loader = new VineLoader();
            var document = loader.Load(data);

            var walker = new ParseTreeWalker();
            var listener = new ElementDescriptionListener();
            walker.Walk(listener, document);

            return listener.Description;
        }
    }
}