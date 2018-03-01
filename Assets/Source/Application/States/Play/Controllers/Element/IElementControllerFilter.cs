using CreateAR.SpirePlayer.IUX;

namespace CreateAR.SpirePlayer
{
    public interface IElementControllerFilter
    {
        bool Include(Element element);
    }
}