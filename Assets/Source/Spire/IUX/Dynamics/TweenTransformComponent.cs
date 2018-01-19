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
            var tweens = Widget.Tweens;

            var localPosition = Vector3.Lerp(
                tweens.ButtonPositionTween.Start,
                tweens.ButtonPositionTween.End,
                tweens.ButtonPositionTween.Curve.Evaluate(tween));

            var localRotation = Vector3.Slerp(
                tweens.ButtonRotationTween.Start,
                tweens.ButtonRotationTween.End,
                tweens.ButtonRotationTween.Curve.Evaluate(tween));

            var localScale = Vector3.Lerp(
                tweens.ButtonScaleTween.Start,
                tweens.ButtonScaleTween.End,
                tweens.ButtonScaleTween.Curve.Evaluate(tween));

            var targetTransform = Transform != null
                ? Transform
                : (Widget != null ? Widget.GameObject.transform : transform);

            targetTransform.localPosition = localPosition;
            targetTransform.localEulerAngles = localRotation;
            targetTransform.localScale = localScale;
        }
    }
}