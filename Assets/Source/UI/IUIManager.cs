using CreateAR.Commons.Unity.Async;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Describes an object that creates and manages UI.
    /// </summary>
    public interface IUIManager
    {
        /// <summary>
        /// Opens a new UI element.
        /// </summary>
        /// <param name="reference">Reference to a UI element.</param>
        /// <param name="stackId">Stack id used to reference element in API.</param>
        /// <returns></returns>
        IAsyncToken<T> Open<T>(UIReference reference, out uint stackId) where T : IUIElement;

        /// <summary>
        /// Moves down the stack, removing UI elements until the element with
        /// the passed in id is on top.
        /// </summary>
        /// <param name="stackId">The id to move along to in the stack.</param>
        void Reveal(uint stackId);

        /// <summary>
        /// Moves down the stack, removing UI elements includig the element
        /// that matches the passed in id.
        /// </summary>
        /// <param name="stackId">The id to move along to in the stack.</param>
        void Close(uint stackId);
    }
}