using UnityEngine;

namespace CreateAR.SpirePlayer
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

        /// <summary>
        /// The associated Unity transform.
        /// </summary>
        Transform Transform { get; }
    }
}