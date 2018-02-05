using CreateAR.Commons.Unity.Async;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Describes an object that listens for adding + removing <c>PropData</c>.
    /// </summary>
    public interface IPropSetUpdateDelegate
    {
        /// <summary>
        /// Called when <c>PropData</c> has been added.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>True iff the delegate successfully added the data.</returns>
        bool Add(PropData data);

        /// <summary>
        /// Called when <c>PropData</c> has been removed.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>True iff the delegate successfully removed the data.</returns>
        bool Remove(PropData data);

        /// <summary>
        /// Has the delegate save data asynchronously.
        /// </summary>
        /// <returns>A token that is resolved upon save.</returns>
        IAsyncToken<Void> Save();
    }
}