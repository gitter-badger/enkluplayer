using UnityEngine;
using UnityEngine.UI;

namespace CreateAR.SpirePlayer.UI
{
    public class ActivatorPrimitive : InteractivePrimitive, IActivatorPrimitive
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
        public WidgetPrimitive FillWidget;

        /// <summary>
        /// Shows/Hides w/ Focus.
        /// </summary>
        public Widget ActivationWidget;

        /// <summary>
        /// Aim Scale Transform.
        /// </summary>
        public WidgetPrimitive AimPrimitive;

        /// <summary>
        /// Spawns when activated
        /// </summary>
        public GameObject ActivationVFX;

        /// <summary>
        /// Fill image visibility
        /// </summary>
        public bool FillImageVisible
        {
            get { return FillWidget.LocalVisible; }
            set { FillWidget.LocalVisible = value; }
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
            if (AimPrimitive != null)
            {
                AimPrimitive.transform.localScale = Vector3.one * aimScale;
            }
        }

        /// <summary>
        /// Sets the aim color.
        /// </summary>
        /// <param name="aimColor"></param>
        public void SetAimColor(Col4 aimColor)
        {
            if (AimPrimitive != null)
            {
                AimPrimitive.LocalColor = aimColor;
            }
        }

        /// <summary>
        /// Activates the spawn VFX
        /// </summary>
        public void Activate()
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
    }
}
