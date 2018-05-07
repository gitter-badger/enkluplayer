using CreateAR.Commons.Unity.Async;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Describes an object that can create IUIElement instances.
    /// </summary>
    public interface IUIElementFactory
    {
        /// <summary>
        /// Create an instance from a UIReference.
        /// </summary>
        /// <param name="reference">The reference to an element.</param>
        /// <param name="id">The returned stack id.</param>
        /// <returns></returns>
        IAsyncToken<IUIElement> Element(UIReference reference, int id);
    }
}