using CreateAR.SpirePlayer.IUX;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Describes an object that can filter elements.
    /// </summary>
    public interface IElementControllerFilter
    {
        /// <summary>
        /// True iff the element should be included in the filtered collection.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns></returns>
        bool Include(Element element);
    }
}