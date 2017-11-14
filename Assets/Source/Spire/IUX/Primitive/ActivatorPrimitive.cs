using UnityEngine;
using UnityEngine.UI;

namespace CreateAR.SpirePlayer.UI
{
    public class ActivatorPrimitive : WidgetPrimitive, IActivatorPrimitive
    {
        /// <summary>
        /// Transform affected by the steadiness of intention
        /// </summary>
        public Transform StabilityTransform;

        /// <summary>
        /// Fills with activation percentage
        /// </summary>
        public Image FillImage;

        /// <summary>
        /// Shows/Hides w/ Focus
        /// </summary>
        public Widget ActivationWidget;

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
    }
}
