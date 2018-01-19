using UnityEngine;

namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// Tweens the color of all materials attached to a widget.
    /// </summary>
    public class TweenTransformComponent : MonoBehaviour, IWidgetComponent
    {
        /// <summary>
        /// Transform.
        /// </summary>
        public Transform Transform;

        /// <summary>
        /// Position information.
        /// </summary>
        [Header("Position")]
        public Vector3 Position0;
        public Vector3 Position1;
        public AnimationCurve PositionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        /// <summary>
        /// Scale information.
        /// </summary>
        [Header("Scale")]
        public Vector3 Scale0 = Vector3.one;
        public Vector3 Scale1 = Vector3.one;
        public AnimationCurve ScaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        /// <summary>
        /// Rotation information.
        /// </summary>
        [Header("Rotation")]
        public Vector3 Rotation0 = Vector3.zero;
        public Vector3 Rotation1 = Vector3.zero;
        public AnimationCurve RotationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        /// <inheritdoc cref="IWidgetComponent"/>
        public Widget Widget { get; set; }
        
        /// <inheritdoc cref="MonoBehaviour"/>
        private void LateUpdate()
        {
            if (Widget == null)
            {
                return;
            }

            var tween = Widget.Tween;

            var localPosition = Vector3.Lerp(
                Position0,
                Position1,
                PositionCurve.Evaluate(tween));

            var localRotation = Vector3.Slerp(
                Rotation0,
                Rotation1,
                RotationCurve.Evaluate(tween));

            var localScale = Vector3.Lerp(
                Scale0,
                Scale1,
                ScaleCurve.Evaluate(tween));

            var targetTransform = Transform != null
                ? Transform
                : (Widget != null ? Widget.GameObject.transform : transform);

            targetTransform.localPosition = localPosition;
            targetTransform.localEulerAngles = localRotation;
            targetTransform.localScale = localScale;
        }
    }
}