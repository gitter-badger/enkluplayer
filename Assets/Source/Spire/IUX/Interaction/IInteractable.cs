using System;
using UnityEngine;

namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// An interface for objects that can be focused upon.
    /// </summary>
    public interface IInteractable : IRaycaster
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
        /// If true, the widget is highlighted.
        /// </summary>
        bool IsHighlighted { get; set; }

        /// <summary>
        /// (IUX PATENT)
        /// A scalar percentage [0..1] representing targeting clarity.
        /// 0 = low clarity - may be aiming at the edge of this.
        /// 1 = high clarity - definitely targeting at center of this.
        /// </summary>
        float Aim { get; }

        /// <summary>
        /// Called when visibility changes.
        /// </summary>
        event Action<IInteractable> OnVisibilityChanged;
    }
}