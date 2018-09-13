using CreateAR.EnkluPlayer.IUX;

namespace CreateAR.EnkluPlayer.Scripting
{
    /// <summary>
    /// Describes an object that can retrieve the JS object for an element.
    /// </summary>
    public interface IElementJsCache
    {
        /// <summary>
        /// Retrieves a JS interface for an element.
        /// </summary>
        /// <param name="element">The element.</param>
        ElementJs Element(Element element);

        /// <summary>
        /// Cleans up elements within.
        /// </summary>
        void Clear();
    }
}