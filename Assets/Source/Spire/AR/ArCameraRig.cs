using UnityEngine;

namespace CreateAR.SpirePlayer.AR
{
    /// <summary>
    /// Camera rig.
    /// </summary>
    public class ArCameraRig : MonoBehaviour
    {
        /// <summary>
        /// Floor anchor.
        /// </summary>
        private ArAnchor _anchor;
        
        /// <summary>
        /// Refernece to the camera, which should be a child.
        /// </summary>
        public Camera Camera;

        /// <summary>
        /// Grid.
        /// </summary>
        public GridRenderer GridRenderer;
        
        /// <summary>
        /// Sets the floor anchor.
        /// </summary>
        /// <param name="anchor">Floor anchor.</param>
        public void SetFloor(ArAnchor anchor)
        {
            if (null != _anchor)
            {
                _anchor.OnChange -= UpdateGrid;
            }

            _anchor = anchor;

            if (null == _anchor)
            {
                return;
            }

            _anchor.OnChange += UpdateGrid;
            
            transform.position = -_anchor.Position.ToVector();
            
            UpdateGrid(_anchor);
        }

        /// <summary>
        /// Updates camera and grid from floor.
        /// </summary>
        /// <param name="anchor">The anchor!</param>
        private void UpdateGrid(ArAnchor anchor)
        {
            // setup grid!
#if UNITY_IOS || UNITY_ANDROID || UNITY_WSA
            Grid.CellSize = 0.5f;
            Grid.GridSize = new Vector2(
                anchor.Extents.x,
                anchor.Extents.z);
#endif
        }
    }
}