using CreateAR.Commons.Unity.Async;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Describes an object that listens for adding + removing <c>PropData</c>
    /// to and from sets.
    /// </summary>
    public interface IPropSetUpdateDelegate
    {
        /// <summary>
        /// Called when <c>PropData</c> has been added.
        /// </summary>
        /// <param name="set">The set to add to.</param>
        /// <param name="data">The data.</param>
        IAsyncToken<Void> Add(PropSet set, PropData data);

        /// <summary>
        /// Called when <c>PropData</c> has been removed.
        /// </summary>
        /// <param name="set">The set to remove from.</param>
        /// <param name="data">The data.</param>
        IAsyncToken<Void> Remove(PropSet set, PropData data);
    }
}