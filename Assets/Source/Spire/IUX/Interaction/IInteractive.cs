using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// An interface for objects that can be focused upon.
    /// </summary>
    public interface IInteractive
    {
        /// <summary>
        /// Access to the Unity Hierarchy (for now...).
        /// </summary>
        GameObject GameObject { get; }

        /// <summary>
        /// True iff the object is visible.
        /// </summary>
        bool IsVisible { get; }

        /// <summary>
        /// True if the object is visible.
        /// </summary>
        bool IsFocused { get; set; }

        /// <summary>
        /// Highlight Priority
        /// </summary>
        int HighlightPriority { get; set; }

        /// <summary>
        /// Handles the engine level interactions.
        /// </summary>
        IInteractivePrimitive InteractivePrimitive { get; }
    }
}