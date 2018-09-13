using UnityEngine;

namespace CreateAR.EnkluPlayer.AR
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
        /// Sets the floor anchor.
        /// </summary>
        /// <param name="anchor">Floor anchor.</param>
        public void SetFloor(ArAnchor anchor)
        {
            _anchor = anchor;

            if (null == _anchor)
            {
                return;
            }
            
            transform.position = -_anchor.Position.ToVector();
        }
    }
}