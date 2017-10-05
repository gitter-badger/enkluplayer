using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Describes a frame of reference for an <c>Anchor</c>.
    /// </summary>
    public interface IAnchorReferenceFrame
    {
        /// <summary>
        /// Forward.
        /// </summary>
        Vector3 Forward { get; }

        /// <summary>
        /// Up.
        /// </summary>
        Vector3 Up { get; }

        /// <summary>
        /// Right.
        /// </summary>
        Vector3 Right { get; }

        /// <summary>
        /// Attaches to frame.
        /// </summary>
        void Attach(Anchor anchor);

        /// <summary>
        /// Called every frame.
        /// </summary>
        /// <param name="dt">Time, in seconds, that have passed since last frame.</param>
        void Update(float dt);
    }
}