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
        /// True if the object is visible.
        /// </summary>
        bool Visible { get; }

        /// <summary>
        /// True if the object is focused.
        /// </summary>
        bool Focused { get; set; }

        /// <summary>
        /// True if the object can be interacted with.
        /// </summary>
        bool Interactable { get; }

        /// <summary>
        /// Highlight Priority.
        /// </summary>
        int HighlightPriority { get; set; }

        /// <summary>
        /// Casts a ray at the interactive object
        /// </summary>
        /// <param name="ray"></param>
        /// <returns></returns>
        bool Cast(Ray ray);
    }
}