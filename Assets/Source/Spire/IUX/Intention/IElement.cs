namespace CreateAR.Spire
{
    /// <summary>
    /// Interface for an IUX element.
    /// </summary>
    public interface IElement
    {
        /// <summary>
        /// True iff the element is visible.
        /// </summary>
        bool IsVisible { get; }

        /// <summary>
        /// When multiple elements compete for highlight, the one with higher
        /// priority wins.
        /// </summary>
        int HighlightPriority { get; }
    }
}