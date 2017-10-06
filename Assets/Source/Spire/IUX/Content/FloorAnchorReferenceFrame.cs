using UnityEngine;

namespace CreateAR.SpirePlayer
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
        
        /// <inheritdoc cref="IAnchorReferenceFrame"/>
        public Vector3 Forward { get { return _intention.Forward; } }

        /// <inheritdoc cref="IAnchorReferenceFrame"/>
        public Vector3 Up { get { return _intention.Up; } }

        /// <inheritdoc cref="IAnchorReferenceFrame"/>
        public Vector3 Right { get { return _intention.Right; } }

        /// <summary>
        /// Constructor.
        /// </summary>
        public FloorAnchorReferenceFrame(IntentionManager intention)
        {
            _intention = intention;
        }

        /// <inheritdoc cref="IAnchorReferenceFrame"/>
        public void Attach(Anchor anchor)
        {
            var origin = _intention.Origin;

            var transform = anchor.transform;
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