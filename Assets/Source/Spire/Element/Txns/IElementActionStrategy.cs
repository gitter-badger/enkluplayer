using CreateAR.SpirePlayer.IUX;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Describes an object that can apply actions to elements.
    /// </summary>
    public interface IElementActionStrategy
    {
        /// <summary>
        /// The root element.
        /// </summary>
        Element Element { get; }
        
        /// <summary>
        /// Applies an action to elements.
        /// </summary>
        /// <param name="action">The action to apply.</param>
        /// <param name="error">The error, if any.</param>
        /// <returns>True iff the action was applied successfully. If false, error will be non null.</returns>
        bool Apply(
            ElementActionData action,
            out string error);
    }
}