namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// Creates elements from data.
    /// </summary>
    public interface IElementFactory
    {
        /// <summary>
        /// Creates an element from a description of that element.
        /// </summary>
        /// <param name="description">The description of the element.</param>
        /// <returns></returns>
        Element Element(ElementDescription description);

        /// <summary>
        /// Craetes an element from a vine.
        /// </summary>
        /// <param name="vine">The raw vine.</param>
        /// <returns></returns>
        Element Element(string vine);
    }
}