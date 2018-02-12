using CreateAR.Commons.Unity.Async;
using CreateAR.SpirePlayer.IUX;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// An interface for an object that handles <c>ElementData</c> updates.
    /// </summary>
    public interface IPropUpdateDelegate
    {
        /// <summary>
        /// Called when element has been updated.
        /// </summary>
        /// <param name="element">The element.</param>
        IAsyncToken<Void> Update(Element element);
    }
}