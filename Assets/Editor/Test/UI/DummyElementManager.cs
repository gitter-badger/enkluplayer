using System.Collections.Generic;
using CreateAR.SpirePlayer.UI;

namespace CreateAR.SpirePlayer.Test.UI
{
    public class DummyElementManager : IElementManager
    {
        private readonly List<IElement> _elements = new List<IElement>();

        public void Add(IElement element)
        {
            _elements.Add(element);
        }
    }
}