using CreateAR.SpirePlayer.IUX;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Creates <c>IElementActionStrategy</c> implementations.
    /// </summary>
    public class ElementActionStrategyFactory : IElementActionStrategyFactory
    {
        /// <summary>
        /// Creates elements.
        /// </summary>
        private readonly IElementFactory _elements;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ElementActionStrategyFactory(IElementFactory elements)
        {
            _elements = elements;
        }

        /// <inheritdoc />
        public IElementActionStrategy Instance(Element root)
        {
            return new ElementActionStrategy(_elements, root);
        }
    }
}