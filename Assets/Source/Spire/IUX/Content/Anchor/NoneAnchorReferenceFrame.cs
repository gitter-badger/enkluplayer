using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Reference frame for None type.
    /// </summary>
    public class NoneAnchorReferenceFrame : IAnchorReferenceFrame
    {
        /// <summary>
        /// Transform of anchor.
        /// </summary>
        private Transform _transform;

        /// <inheritdoc cref="IAnchorReferenceFrame"/>
        public Vec3 Forward { get { return _transform.forward.ToVec(); } }

        /// <inheritdoc cref="IAnchorReferenceFrame"/>
        public Vec3 Up { get { return _transform.up.ToVec(); } }

        /// <inheritdoc cref="IAnchorReferenceFrame"/>
        public Vec3 Right { get { return _transform.right.ToVec(); } }

        /// <inheritdoc cref="IAnchorReferenceFrame"/>
        public void Attach(Anchor anchor)
        {
            _transform = anchor.transform;
        }

        /// <inheritdoc cref="IAnchorReferenceFrame"/>
        public void Update(float dt)
        {
            //
        }
    }
}