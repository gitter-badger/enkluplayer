using CreateAR.Commons.Unity.Async;
using CreateAR.SpirePlayer.IUX;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Describes an object that listens for adding + removing <c>ElementData</c>
    /// to and from sets.
    /// </summary>
    public interface IPropSetUpdateDelegate
    {
        /// <summary>
        /// Called when <c>ElementData</c> has been added.
        /// </summary>
        /// <param name="set">The set to add to.</param>
        /// <param name="data">The data.</param>
        IAsyncToken<Void> Add(PropSet set, ElementData data);

        /// <summary>
        /// Called when <c>ElementData</c> has been removed.
        /// </summary>
        /// <param name="set">The set to remove from.</param>
        /// <param name="element">The element.</param>
        IAsyncToken<Void> Remove(PropSet set, Element element);
    }
}