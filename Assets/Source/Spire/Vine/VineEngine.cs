using Antlr4.Runtime.Tree;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Vine;
using CreateAR.SpirePlayer.IUX;
using UnityEngine;

namespace CreateAR.SpirePlayer.Vine
{
    public class VineEngine
    {
        public ElementDescription Parse(string data)
        {
            var description = new ElementDescription();
            var loader = new VineLoader();
            var document = loader.Load(data);

            var walker = new ParseTreeWalker();
            walker.Walk(new ElementDescriptionListener(description), document);

            return description;
        }
    }
}