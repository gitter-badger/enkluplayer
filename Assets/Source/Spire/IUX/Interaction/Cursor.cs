using CreateAR.Commons.Unity.Logging;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Messaging;
using CreateAR.SpirePlayer.UI;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Manages the cursor rendering
    /// </summary>
    public class Cursor : Widget
    {
        /// <summary>
        /// Dependencies
        /// </summary>
        public IIntentionManager Intention { get; private set; }

        /// <summary>
        /// Activator primitive
        /// </summary>
        private IReticle _reticle;

        /// <summary>
        /// A measure of aim over time. 
        /// </summary>
        private float _spread;

        /// <summary>
        /// How well is the user aiming.
        /// </summary>
        private float _aim;

        /// <summary>
        /// Scale of the points.
        /// </summary>
        private float _scale = 1.0f;

        /// <summary>
        /// Measured in theta/second.
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
        /// Constructor.
        /// </summary>
        public void Initialize(
            IWidgetConfig config,
            ILayerManager layers,
            ITweenConfig tweens,
            IColorConfig colors,
            IMessageRouter messages,
            IIntentionManager intention)
        {
            Intention = intention;
            Initialize(config, layers, tweens, colors, messages);
        }

        /// <summary>
        /// Initialization
        /// </summary>
        protected override void LoadInternal()
        {
            base.LoadInternal();

            _reticle = FindOne("reticle") as IReticle;

            _cursorDistance = Config.GetDefaultDistanceForCursor();
        }

        /// <summary>
        /// Frame based update
        /// </summary>
        protected override void LateUpdateInternal()
        {
            base.LateUpdateInternal();

            var deltaTime
                = Time
                    .smoothDeltaTime;

            UpdateAim(deltaTime);
            UpdatePosition(deltaTime);
            UpdateSpin(deltaTime);
            UpdateVisibility();
            UpdateReticle(deltaTime);
        }
        
        /// <summary>
        /// Updates Widget Visibility
        /// </summary>
        private void UpdateVisibility()
        {
            LocalVisible
                = !Intention.InputDisabled;
            // && Widgets.GetVisibleCount<InteractableWidget>() > 0;
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
                    ? Config.GetFillRateMultiplierFromAim(_aim)
                      * Config.GetFillRateMultiplierFromStability(Intention.Stability)
                      * Config.GetReticleSpinRateForCursor()
                      * Mathf.PI
                      * 2.0f
                    : 0.0f;

            var tweenDuration
                = Tweens
                    .DurationSeconds(
                        _aim > Mathf.Epsilon
                            ? TweenIn
                            : TweenOut);

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

            var interactive = Intention.Focus;
            if (_aim > Mathf.Epsilon)
            {
                var aimMagnet = Config.GetMagnetFromAim(_aim);

                cursorPosition
                    = Vec3.Lerp(
                        cursorPosition,
                        interactive.GameObject.transform.position.ToVec(),
                        aimMagnet);
            }

            GameObject.transform.position = cursorPosition.ToVector();
            GameObject.transform.forward = -eyeDirection.ToVector();
        }

        /// <summary>
        /// Updates the cursor distance
        /// </summary>
        /// <param name="deltaTime"></param>
        /// <param name="eyePosition"></param>
        private void UpdateCursorDistance(float deltaTime, Vec3 eyePosition)
        {
            var targetFocusDistance = Config.GetDefaultDistanceForCursor();

            var interactive = Intention.Focus;
            if (interactive != null
             && interactive.GameObject != null)
            {
                // focus on the focus widget
                var eyeDeltaToFocusWidget
                    = interactive.GameObject.transform.position.ToVec()
                    - eyePosition;
                targetFocusDistance
                    = eyeDeltaToFocusWidget
                        .Magnitude;
                var activator
                    = interactive as IActivator;
                if (activator != null)
                {
                    targetFocusDistance
                        = targetFocusDistance
                          - activator.Radius;
                }
                
            }

            var tweenDuration
                = Tweens
                    .DurationSeconds(
                        interactive != null
                            ? TweenIn
                            : TweenOut);

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

            var interactive = Intention.Focus;
            var activator = (interactive);
            if (activator != null)
            {
                _aim = activator.Aim;
            }

            var buttonScale
                = (interactive != null 
                && interactive.GameObject != null)
                    ? interactive.GameObject.transform.lossyScale.x
                    : 1.0f;

            _spread 
                = Config.GetReticleSpreadFromAim(_aim)
                * buttonScale;

            LocalColor = Config.GetReticleColorFromAim(_aim);

            _scale = Config.GetReticleScaleFromAim(_aim)
                   * buttonScale;
        }

        /// <summary>
        /// Updates the renderer
        /// </summary>
        private void UpdateReticle(float deltaTime)
        {
            _reticle.Scale = _scale;
            _reticle.Rotation = _thetaRadians;
            _reticle.Spread = _spread;

            var isAiming = Intention.Focus != null;
            var tweenDuration
                = Tweens
                    .DurationSeconds(
                        isAiming
                            ? TweenType.Instant
                            : TweenOut);

            var tweenLerp
                = tweenDuration > Mathf.Epsilon
                    ? deltaTime / tweenDuration
                    : 1.0f;

            _reticle.CenterAlpha
                = Mathf
                    .Lerp(
                        _reticle.CenterAlpha,
                        isAiming ? 0.0f : 1.0f,
                        tweenLerp);
        }
    }
}
