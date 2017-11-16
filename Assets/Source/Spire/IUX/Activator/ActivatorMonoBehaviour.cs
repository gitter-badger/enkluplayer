using CreateAR.Commons.Unity.Logging;
using UnityEngine;
using UnityEngine.UI;

namespace CreateAR.SpirePlayer.UI
{
    public class ActivatorMonoBehaviour : WidgetMonoBehaviour, IActivator
    {
        /// <summary>
        /// Transform affected by the steadiness of intention.
        /// </summary>
        public Transform StabilityTransform;

        /// <summary>
        /// Fills with activation percentage.
        /// </summary>
        public Image FillImage;

        /// <summary>
        /// Fill Widget
        /// </summary>
        public WidgetMonoBehaviour FillWidget;

        /// <summary>
        /// Shows/Hides w/ Focus.
        /// </summary>
        public Widget ActivationWidget;

        /// <summary>
        /// Aim Scale Transform.
        /// </summary>
        public WidgetMonoBehaviour AimMonoBehaviour;

        /// <summary>
        /// Spawns when activated
        /// </summary>
        public GameObject ActivationVFX;

        /// <summary>
        /// For gaining focus.
        /// </summary>
        public BoxCollider FocusCollider;

        /// <summary>
        /// For losing focus.
        /// </summary>
        public BoxCollider BufferCollider;

        /// <summary>
        /// Fill image visibility
        /// </summary>
        public bool FillImageVisible
        {
            get { return FillWidget.LocalVisible; }
            set { FillWidget.LocalVisible = value; }
        }

        /// <summary>
        /// Frame widget
        /// </summary>
        public IWidget Frame
        {
            get { return null; }
        }

        /// <summary>
        /// Initialization
        /// </summary>
        protected void LoadInternal()
        {
            GenerateBufferCollider();
        }

        /// <summary>
        /// Updates the rotation and scale of the stability transform.
        /// </summary>
        /// <param name="degress"></param>
        public void SetStabilityRotation(float degress)
        {
            var focusTween 
                = ActivationWidget != null 
                ? ActivationWidget.Tween 
                : 1.0f;

            StabilityTransform.localRotation = Quaternion.Euler(0, 0, degress);
            StabilityTransform.localScale = Vector3.one * focusTween;
        }

        /// <summary>
        /// Updates the fill for the activation.
        /// </summary>
        /// <param name="percent"></param>
        public void SetActivationFill(float percent)
        {
            if (FillImage != null)
            {
                FillImage.fillAmount = percent;
            }
        }

        /// <summary>
        /// Sets the aim scale.
        /// </summary>
        /// <param name="aimScale"></param>
        public void SetAimScale(float aimScale)
        {
            if (AimMonoBehaviour != null)
            {
                AimMonoBehaviour.transform.localScale = Vector3.one * aimScale;
            }
        }

        /// <summary>
        /// Sets the aim color.
        /// </summary>
        /// <param name="aimColor"></param>
        public void SetAimColor(Col4 aimColor)
        {
            if (AimMonoBehaviour != null)
            {
                AimMonoBehaviour.LocalColor = aimColor;
            }
        }

        /// <summary>
        /// Activates the spawn VFX
        /// </summary>
        public void ShowActivateVFX()
        {
            if (ActivationVFX != null)
            {
                var spawnGameObject
                    = UnityEngine
                        .Object
                        .Instantiate(ActivationVFX,
                            gameObject.transform.position,
                            gameObject.transform.rotation);
                spawnGameObject.SetActive(true);
            }
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
