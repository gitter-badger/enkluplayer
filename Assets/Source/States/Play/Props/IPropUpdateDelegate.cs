using CreateAR.Commons.Unity.Async;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// An interface for an object that handles <c>PropData</c> updates.
    /// </summary>
    public interface IPropUpdateDelegate
    {
        /// <summary>
        /// Called when <c>PropData</c> has been updated.
        /// </summary>
        /// <param name="data">The data.</param>
        IAsyncToken<Void> Update(PropData data);
    }
}