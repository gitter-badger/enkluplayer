using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.UI;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    public class InteractivePrimitive : WidgetPrimitive, IInteractivePrimitive
    {
        /// <summary>
        /// For gaining focus.
        /// </summary>
        public BoxCollider FocusCollider;

        /// <summary>
        /// For losing focus.
        /// </summary>
        public BoxCollider BufferCollider;
        
        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="widget"></param>
        public override void Load(IWidget widget)
        {
            base.Load(widget);

            GenerateBufferCollider();
        }

        /// <summary>
        /// Returns the radius of the widget.
        /// </summary>
        public float GetBoundingRadius()
        {
            var radius = 1f;
            if (null != FocusCollider)
            {
                var size = FocusCollider.size;
                var scale = FocusCollider.transform.lossyScale;
                var scaledSize = new Vector3(
                    size.x * scale.x,
                    size.y * scale.y,
                    size.z * scale.z);
                radius = 0.5f * (scaledSize.x + scaledSize.y + scaledSize.z) / 3f;
            }

            return radius;
        }

        /// <summary>
        /// Generate buffer collider
        /// </summary>
        private void GenerateBufferCollider()
        {
            if (FocusCollider == null)
            {
                Log.Error(this, "Missing FocusCollider for AutoGenerateBufferCollider!");
                return;
            }

            if (BufferCollider == null)
            {
                BufferCollider = gameObject.AddComponent<BoxCollider>();
            }

            const float AUTO_GEN_BUFFER_FACTOR = 2.0f;
            BufferCollider.size = FocusCollider.size * AUTO_GEN_BUFFER_FACTOR;
        }

        /// <summary>
        /// Returns true if the primitive is targeted.
        /// </summary>
        /// <param name="ray"></param>
        /// <returns></returns>
        public bool IsTargeted(Ray ray)
        {
            if (BufferCollider != null)
            {
                RaycastHit hitInfo;
                if (BufferCollider.Raycast(ray, out hitInfo, float.PositiveInfinity))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Enables/disables interaction on the primitive.
        /// </summary>
        /// <param name="enable"></param>
        public void SetInteractionEnabled(bool enable, bool bufferEnabled)
        {
            if (FocusCollider != null)
            {
                FocusCollider.enabled = enable;
            }

            if (BufferCollider != null)
            {
                BufferCollider.enabled = bufferEnabled;
            }
        }
    }
}
