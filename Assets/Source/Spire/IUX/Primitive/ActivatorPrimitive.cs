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
        /// Shows/Hides w/ Focus.
        /// </summary>
        public Widget ActivationWidget;

        /// <summary>
        /// Aim Scale Transform.
        /// </summary>
        public Transform AimTransform;

        /// <summary>
        /// Updates the rotation and scale of the stability transform
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
        /// Updates the fill for the activation
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
        /// Sets the aim scale
        /// </summary>
        /// <param name="aimScale"></param>
        public void SetAimScale(float aimScale)
        {
            if (AimTransform != null)
            {
                AimTransform.localScale = Vector3.one * aimScale;
            }
        }
    }
}
