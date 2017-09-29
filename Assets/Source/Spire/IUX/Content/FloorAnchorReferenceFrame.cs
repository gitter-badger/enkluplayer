using UnityEngine;

namespace CreateAR.Spire
{
    /// <summary>
    /// Frame of reference for the Floor.
    /// </summary>
    public class FloorAnchorReferenceFrame : IAnchorReferenceFrame
    {
        /// <summary>
        /// TODO: What are these?
        /// </summary>
        public const float FloorY = -1.15f;
        public static bool UsedDefaultFloorY = true;

        /// <summary>
        /// Dependencies.
        /// </summary>
        private readonly IntentionManager _intention;

        /// <summary>
        /// The <c>Anchor</c> this frame of reference is for.
        /// </summary>
        private readonly Anchor _anchor;

        /// <inheritdoc cref="IAnchorReferenceFrame"/>
        public Vector3 Forward { get { return _intention.Forward; } }

        /// <inheritdoc cref="IAnchorReferenceFrame"/>
        public Vector3 Up { get { return _intention.Up; } }

        /// <inheritdoc cref="IAnchorReferenceFrame"/>
        public Vector3 Right { get { return _intention.Right; } }

        /// <summary>
        /// Constructor.
        /// </summary>
        public FloorAnchorReferenceFrame(
            IntentionManager intention,
            Anchor anchor)
        {
            _intention = intention;
            _anchor = anchor;
        }

        /// <inheritdoc cref="IAnchorReferenceFrame"/>
        public void Attach()
        {
            var origin = _intention.Origin;

            var transform = _anchor.transform;
            transform.SetParent(null);
            transform.position = origin;
        }

        /// <inheritdoc cref="IAnchorReferenceFrame"/>
        public void Update(float dt)
        {
            //
        }
    }
}