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
            UpdateFromAnchor(anchor);
        }

        /// <summary>
        /// Updates camera and grid from floor.
        /// </summary>
        /// <param name="anchor">The anchor!</param>
        private void UpdateFromAnchor(ArAnchor anchor)
        {
            transform.position = -anchor.Position.ToVector();

            // setup grid!
            Grid.CellSize = 0.5f;
            Grid.GridSize = new Vector2(
                anchor.Extents.x,
                anchor.Extents.z);
            Grid.Enabled = true;
        }
    }
}