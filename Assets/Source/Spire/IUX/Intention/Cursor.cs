using CreateAR.Commons.Unity.Logging;
using System.Collections.Generic;
using CreateAR.SpirePlayer.UI;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /*
    /// <summary>
    /// Manages the cursor rendering
    /// </summary>
    public class Cursor : Widget
    {
        /// <summary>
        /// Spread 
        /// </summary>
        private float _spread = 0.0f;

        /// <summary>
        /// Scale of the points
        /// </summary>
        private float _scale = 1.0f;

        /// <summary>
        /// How well is the user aiming
        /// </summary>
        private float _aim;

        /// <summary>
        /// Measured in theta/second
        /// </summary>
        private float _angularVelocityRadians;

        /// <summary>
        /// Spin theta
        /// </summary>
        private float _thetaRadians;

        /// <summary>
        /// Current cursor distance
        /// </summary>
        private float _cursorDistance;

        /// <summary>
        /// Default Cursor Distance
        /// </summary>
        public float DefaultFocalDistance = 2.0f;

        /// <summary>
        /// How fast does the cursor state change
        /// </summary>
        public TweenType AimTweenType = TweenType.Instant;

        /// <summary>
        /// Tween for gaining focus
        /// </summary>
        public TweenType GainFocusTween = TweenType.Responsive;

        /// <summary>
        /// Tween for losing focus
        /// </summary>
        public TweenType LostFocusTween = TweenType.Deliberate;

        /// <summary>
        /// Spread when activating
        /// </summary>
        public AnimationCurve AimSpread = new AnimationCurve();
        
        /// <summary>
        /// Modifies Aim Scale
        /// </summary>
        public float AimSpreadMultiplier = 1.0f;

        /// <summary>
        /// Defines how magnetty the cursor is
        /// </summary>
        public AnimationCurve AimMagnet = new AnimationCurve();

        /// <summary>
        /// How aim affects the scale of points
        /// </summary>
        public AnimationCurve AimScale = new AnimationCurve();

        /// <summary>
        /// Modifies Aim Scale
        /// </summary>
        public float AimScaleMultiplier = 1.0f;

        /// <summary>
        /// How aim affects color of points
        /// </summary>
        public Gradient AimColor = new Gradient();

        /// <summary>
        /// Spins per second
        /// </summary>
        public float SpinSpeed = 2.0f;

        /// <summary>
        /// Widgets
        /// </summary>
        public List<Widget> Points = new List<Widget>();

        /// <summary>
        /// Center point
        /// </summary>
        public Widget CenterPoint;

        /// <summary>
        /// Initialization
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            _cursorDistance = DefaultFocalDistance;
        }

        /// <summary>
        /// Frame based update
        /// </summary>
        public void LateUpdate()
        {
            var deltaTime
                = Time
                    .smoothDeltaTime;

            UpdateAim(deltaTime);
            UpdatePosition(deltaTime);
            UpdateSpin(deltaTime);
            UpdatePoints(deltaTime);
            UpdateVisibility();
        }
        
        /// <summary>
        /// Updates Widget Visibility
        /// </summary>
        private void UpdateVisibility()
        {
            LocalVisible
                = !Intention.InputDisabled
               && Widgets.GetVisibleCount<InteractableWidget>() > 0;
        }

        /// <summary>
        /// Spin speed
        /// </summary>
        /// <param name="deltaTime"></param>
        private void UpdateSpin(float deltaTime)
        {
            if (_spread < 0.001f)
            {
                // reset theta
                _thetaRadians = 0.0f;
                _angularVelocityRadians = 0.0f;
                return;
            }

            var targetAngularVelocityRadians
                = _aim > Mathf.Epsilon
                    ? Config
                          .AimFillMultiplier
                          .Evaluate(_aim)
                      * Config
                          .SteadinessFillMultiplier
                          .Evaluate(Intention.Steadiness)
                      * SpinSpeed
                      * Mathf.PI
                      * 2.0f
                    : 0.0f;

            var tweenDuration
                = Tweens
                    .DurationSeconds(
                        _aim > Mathf.Epsilon
                            ? GainFocusTween
                            : LostFocusTween);

            var tweenLerp
                = tweenDuration > Mathf.Epsilon
                    ? deltaTime / tweenDuration
                    : 1.0f;

            _angularVelocityRadians
                = Mathf.Lerp(
                    _angularVelocityRadians,
                    targetAngularVelocityRadians,
                    tweenLerp);

            _thetaRadians
                += _angularVelocityRadians
                   * deltaTime;
        }

        /// <summary>
        /// Updates the position
        /// </summary>
        private void UpdatePosition(float deltaTime)
        {
            var eyePosition
                = Intention
                    .Origin;

            var eyeDirection
                = Intention
                    .Forward;

            UpdateCursorDistance(
                deltaTime,
                eyePosition);

            var cursorPosition
                = eyePosition
                  + _cursorDistance
                  * eyeDirection;

            var focusable = Intention.Focus;
            if (_aim > Mathf.Epsilon
             && focusable != null
             && focusable.FocusCollider != null)
            {
                var aimMagnet
                    = AimMagnet
                        .Evaluate(_aim);

                cursorPosition
                    = Vec3.Lerp(
                        cursorPosition,
                        focusable.FocusCollider.transform.position.ToVec(),
                        aimMagnet);
            }

            transform.position = cursorPosition.ToVector();
            transform.forward = -eyeDirection.ToVector();
        }

        /// <summary>
        /// Updates the cursor distance
        /// </summary>
        /// <param name="deltaTime"></param>
        /// <param name="eyePosition"></param>
        private void UpdateCursorDistance(float deltaTime, Vec3 eyePosition)
        {
            var targetFocusDistance
                = DefaultFocalDistance;

            var focusable = Intention.Focus;
            if (focusable != null
             && focusable.FocusCollider != null)
            {
                // focus on the focus widget
                var eyeDeltaToFocusWidget
                    = focusable
                          .FocusCollider
                          .transform
                          .position
                          .ToVec()
                      - eyePosition;
                var eyeDistanceToFocusWidget
                    = eyeDeltaToFocusWidget
                        .Magnitude;
                targetFocusDistance
                    = eyeDistanceToFocusWidget
                      - focusable
                          .Radius;
            }

            var tweenDuration
                = Tweens
                    .DurationSeconds(
                        focusable != null
                            ? GainFocusTween
                            : LostFocusTween);

            var tweenLerp
                = tweenDuration > Mathf.Epsilon
                    ? deltaTime / tweenDuration
                    : 1.0f;

            _cursorDistance
                = Mathf
                    .Lerp(
                        _cursorDistance,
                        targetFocusDistance,
                        tweenLerp);
        }

        /// <summary>
        /// Updates the spread of the cursor
        /// </summary>
        /// <param name="deltaTime"></param>
        private void UpdateAim(float deltaTime)
        {
            _aim = 0.0f;
            var focusable = Intention.Focus;
            var aimableWidget
                = focusable != null 
               && focusable.FocusCollider.transform != null
                    ? focusable.FocusCollider.transform.GetComponent<AimableWidget>()
                    : null;
            if (aimableWidget != null)
            {
                _aim = aimableWidget.Aim;
            }

            var buttonScale
                = (aimableWidget != null)
                    ? aimableWidget.transform.lossyScale.x
                    : 1.0f;
            if (float.IsNaN(buttonScale)
             || float.IsInfinity(buttonScale))
            {
                Log.Error(this, "Invalid Button Scale[{0}], resetting...", buttonScale);
                buttonScale = 1.0f;
            }

            var tweenDuration
                = Tweens
                    .DurationSeconds(AimTweenType);

            var tweenLerp
                = tweenDuration > Mathf.Epsilon
                    ? deltaTime / tweenDuration
                    : 1.0f;

            var targetSpread
                = AimSpreadMultiplier
                  * AimSpread
                      .Evaluate(_aim)
                  * buttonScale;
            if (float.IsNaN(targetSpread)
                || float.IsInfinity(targetSpread))
            {
                Log.Error(this, "Invalid targetSpread[{0}], resetting...", targetSpread);
                targetSpread = 1.0f;
            }

            _spread
                = Mathf
                    .Lerp(
                        _spread,
                        targetSpread,
                        tweenLerp);

            if (float.IsNaN(_spread)
                || float.IsInfinity(_spread))
            {
                Log.Error(this, "Invalid _spread[{0}], resetting...", _spread);
                _spread = 1.0f;
            }

            var targetColor
                = AimColor
                    .Evaluate(_aim);

            LocalColor
                = Color
                    .Lerp(
                        LocalColor,
                        targetColor,
                        tweenLerp);

            var targetScale
                = AimScaleMultiplier
                  * AimScale
                      .Evaluate(_aim)
                  * buttonScale;

            _scale
                = Mathf.Lerp(
                    _scale,
                    targetScale,
                    tweenLerp);

            if (float.IsNaN(_scale)
             || _scale < Mathf.Epsilon)
            {
                _scale = 1.0f;
            }
        }

        /// <summary>
        /// Updates the point widgets
        /// </summary>
        /// <param name="deltaTime"></param>
        private void UpdatePoints(float deltaTime)
        {
            for (int i = 0, count = Points.Count; i < count; ++i)
            {
                var point
                    = Points[i];
                if (point != null)
                {
                    var theta
                        = Mathf.PI
                          * 2.0f
                          * i
                          / count
                          + _thetaRadians;

                    var pointLocalPosition
                        = new Vector3(
                            Mathf.Sin(theta) * _spread,
                            Mathf.Cos(theta) * _spread,
                            0.0f);

                    point
                        .transform
                        .localPosition
                        = pointLocalPosition;

                    point
                        .transform
                        .localScale
                        = Vector3.one
                          * _scale;

                    point
                        .transform
                        .localRotation
                        = Quaternion.AngleAxis(Mathf.Rad2Deg * -theta, Vector3.forward);
                }
            }

            if (CenterPoint != null)
            {
                CenterPoint
                    .LocalVisible = Intention.Focus == null;

                CenterPoint
                        .transform
                        .localScale
                    = Vector3.one
                      * _scale;
            }
        }
    }*/
}
