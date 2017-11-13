using UnityEngine;

namespace CreateAR.SpirePlayer.AR
{
    /// <summary>
    /// Camera rig.
    /// </summary>
    public class ArCameraRig : MonoBehaviour
    {
        /// <summary>
        /// Refernece to the camera, which should be a child.
        /// </summary>
        public Camera Camera;

        /// <summary>
        /// Grid.
        /// </summary>
        public Grid Grid;

        /// <summary>
        /// Sets the floor anchor.
        /// </summary>
        /// <param name="anchor">Floor anchor.</param>
        public void SetFloor(ArAnchor anchor)
        {
            transform.position = -anchor.Position;

            //Grid.Offset = anchor.Position;
        }
    }
}