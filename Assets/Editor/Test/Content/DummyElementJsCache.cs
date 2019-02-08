using CreateAR.EnkluPlayer.IUX;
using CreateAR.EnkluPlayer.Scripting;

namespace CreateAR.EnkluPlayer.Test.Scripting
{
    public class DummyElementJsCache : IElementJsCache
    {
        public ElementJs Element(Element element)
        {
            return null;
        }

        public void Clear()
        {
            
        }
    }
}