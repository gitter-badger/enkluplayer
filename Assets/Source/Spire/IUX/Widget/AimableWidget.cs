using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// 
    /// </summary>
    public class AimableWidget : InteractableWidget
    {
        /// <summary>
        /// Current aim percentage.
        /// </summary>
        private float _aim;

        /// <summary>
        /// Displayed when aiming at the button.
        /// </summary>
        public Widget AimFeedbackWidget;

        /// <summary>
        /// Minimum scale of the Aim Widget.
        /// </summary>
        public float AimFeedbackWidgetScaleMin = 0.4f;

        /// <summary>
        /// Maximum scale of the Aim Widget.
        /// </summary>
        public float AimFeedbackWidgetScaleMax = 0.6f;

        /// <summary>
        /// Color of the aim widget as a function of aim percentage.
        /// </summary>
        public Gradient AimFeedbackWidgetColor;

        /// <summary>
        /// Aim percentage.
        /// </summary>
        public float Aim
        {
            get { return _aim; }
            set { _aim = value; }
        }

        /// <summary>
        /// Updates the aim of the activation.
        /// </summary>
        public override void Update()
        {
            base.Update();

            if (!IsFocused
             || !IsInteractable)
            {
                _aim = 0.0f;
            }
            else
            {
                UpdateAim();
            }

            if (AimFeedbackWidget != null)
            {
                var aimFeedbackWidgetScale
                    = Mathf.Lerp(AimFeedbackWidgetScaleMin, AimFeedbackWidgetScaleMax, _aim);
                AimFeedbackWidget.transform.localScale 
                    = Vector3.one 
                    * aimFeedbackWidgetScale;
                AimFeedbackWidget.LocalColor 
                    = AimFeedbackWidgetColor.Evaluate(_aim);
            }
        }

        /// <summary>
        /// Updates the aim as a function of focus towards the center of the widget.
        /// </summary>
        private void UpdateAim()
        {
            var eyePosition
                = Intention
                    .Origin;
            var eyeDirection
                = Intention
                    .Forward;
            var delta
                = transform
                      .position.ToVec() - eyePosition;
            var directionToButton
                = delta
                    .Normalized;

            var eyeDistance
                = delta
                    .Magnitude;
            var radius
                = Radius;

            var maxTheta
                = Mathf.Atan2(radius, eyeDistance);

            var cosTheta
                = Vec3
                    .Dot(
                        directionToButton,
                        eyeDirection);
            var theta
                = Mathf.Approximately(cosTheta, 1.0f)
                    ? 0.0f
                    : Mathf.Acos(cosTheta);

            _aim
                = Mathf.Approximately(maxTheta, 0.0f)
                    ? 0.0f
                    : 1.0f
                      - Mathf
                          .Clamp01(
                              Mathf
                                  .Abs(
                                      theta / maxTheta));
        }
    }
}
