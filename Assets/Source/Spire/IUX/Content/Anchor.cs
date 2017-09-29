using UnityEngine;
using UnityEngine.VR.WSA;

namespace CreateAR.Spire
{
    /// <summary>
    /// Positions self relative to an <c>IAnchorReferenceFrame</c>.
    /// </summary>
    public class Anchor : MonoBehaviour
    {
        /// <summary>
        /// Frame of reference for anchoring.
        /// </summary>
        private IAnchorReferenceFrame _referenceFrame;
        
        /// <summary>
        /// Date for this anchor.
        /// </summary>
        public AnchorData Data { get; private set; }
        
        /// <summary>
        /// Useful ToString.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("[Anchor name={0}]", transform.name);
        }

        /// <summary>
        /// Initializes the Anchor.
        /// </summary>
        public void Initialize(
            IAnchorReferenceFrame referenceFrame,
            AnchorData data)
        {
            _referenceFrame = referenceFrame;
            Data = data;

            _referenceFrame.Attach();

            RefreshLocalTransform();

            ApplyOffset();

            if (Data.Spatial)
            {
                if (null == GetComponent<WorldAnchor>())
                {
                    gameObject.AddComponent<WorldAnchor>();
                }
            }
        }

        /// <summary>
        /// Adjusts every frame.
        /// </summary>
        private void Update()
        {
            if (null != _referenceFrame)
            {
                _referenceFrame.Update(Time.deltaTime);
            }
        }
        
        /// <summary>
        /// Adjusts the local transform according to data.
        /// </summary>
        private void RefreshLocalTransform()
        {
            if (Data.Rotation == RotationType.Orient)
            {
                var forward = _referenceFrame.Forward;
                forward.y = 0;
                forward.Normalize();
                transform.forward = forward;
            }
            else if (Data.Rotation == RotationType.Identity)
            {
                transform.localRotation = Quaternion.identity;
            }

            if (Data.Scale == ScaleType.Identity)
            {
                transform.localScale = Vector3.one;
            }
        }
        
        /// <summary>
        /// Apply local offsets.
        /// </summary>
        private void ApplyOffset()
        {
            transform.position += Data.WorldOffset;

            if (!Data.ViewOffset.Approximately(Vector3.zero))
            {
                var viewForward = _referenceFrame.Forward;
                viewForward.y = 0.0f;
                viewForward.Normalize();
                
                var viewRight = _referenceFrame.Right;
                var viewUp = Vector3.Cross(viewForward, viewRight).normalized;
                viewRight = Vector3.Cross(viewUp, viewForward).normalized;

                transform.position = transform.position
                       + Data.ViewOffset.x * viewRight
                       + Data.ViewOffset.y * viewUp
                       + Data.ViewOffset.z * viewForward;
            }

            transform.localPosition += Data.Offset;
        }
    }
}