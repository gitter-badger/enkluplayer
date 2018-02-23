using CreateAR.SpirePlayer.IUX;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Interface for creating <c>IElementActionStrategy</c> implementations.
    /// </summary>
    public interface IElementActionStrategyFactory
    {
        /// <summary>
        /// Creates an <c>IElementActionStrategy</c>.
        /// </summary>
        /// <param name="root">The root element.</param>
        /// <returns></returns>
        IElementActionStrategy Instance(Element root);
    }
}