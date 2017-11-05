using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// An interface for objects that can be focused upon.
    /// </summary>
    public interface IFocusable
    {
        /// <summary>
        /// True iff the object is visible.
        /// </summary>
        bool IsVisible { get; }

        /// <summary>
        /// Collider for receiving focus.
        /// </summary>
        Collider FocusCollider { get; }

        /// <summary>
        /// Collider for unfocusing.
        /// </summary>
        Collider UnfocusCollider { get; }

        /// <summary>
        /// Focal radius
        /// </summary>
        float Radius { get; }
    }
}