using UnityEngine;

namespace CreateAR.EnkluPlayer.IUX
{
    /// <summary>
    /// Manages the cursor rendering
    /// </summary>
    public class Cursor : Widget
    {
        /// <summary>
        /// Dependencies.
        /// </summary>
        private readonly WidgetConfig _config;
        private readonly TweenConfig _tweens;
        private readonly IIntentionManager _intention;
        private readonly IInteractionManager _interaction;
        private readonly IPrimitiveFactory _primitives;

        /// <summary>
        /// Reticle primitive.
        /// </summary>
        private ReticlePrimitive _reticle;

        /// <summary>
        /// A measure of aim over time. 
        /// </summary>
        private float _spread;

        /// <summary>
        /// How well is the user aiming.
        /// </summary>
        private float _aim = -1;

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
        /// Center alpha of reticle.
        /// </summary>
        private float _reticleCenterAlpha = 0f;

        /// <summary>
        /// Constructor.
        /// </summary>
        public Cursor(
            GameObject gameObject,
            WidgetConfig config,
            ILayerManager layers,
            TweenConfig tweens,
            ColorConfig colors,
            IIntentionManager intention,
            IInteractionManager interaction,
            IPrimitiveFactory primitives)
            : base(
                gameObject,
                layers,
                tweens,
                colors)
        {
            _config = config;
            _tweens = tweens;
            _intention = intention;
            _interaction = interaction;
            _primitives = primitives;
        }

        /// <summary>
        /// Initialization
        /// </summary>
        protected override void LoadInternalAfterChildren()
        {
            base.LoadInternalAfterChildren();

            _reticle = _primitives.Reticle();
            _reticle.Parent = this;

            _cursorDistance = _config.GetDefaultDistanceForCursor();
        }

        /// <summary>
        /// Frame based update
        /// </summary>
        protected override void LateUpdateInternal()
        {
            base.LateUpdateInternal();

            var deltaTime = Time.smoothDeltaTime;

            UpdateAim();
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
            var visible = _interaction.Visible.Count > 0;

            // Only show the cursor when hovering over an Interactable on Hololens
            if (DeviceHelper.IsHoloLens())
            {
                visible = visible && _aim >= 0;
            }

            LocalVisible = visible;
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

            var targetAngularVelocityRadians = _aim > Mathf.Epsilon
                ? _config.GetFillRateMultiplierFromAim(_aim)
                    * _config.GetFillRateMultiplierFromStability(_intention.Stability)
                    * _config.GetReticleSpinRateForCursor()
                    * Mathf.PI
                    * 2.0f
                : 0.0f;

            var tweenDuration = _tweens.DurationSeconds(
                _aim > Mathf.Epsilon
                    ? TweenIn
                    : TweenOut);

            var tweenLerp = tweenDuration > Mathf.Epsilon
                ? deltaTime / tweenDuration
                : 1.0f;

            _angularVelocityRadians = Mathf.Lerp(
                _angularVelocityRadians,
                targetAngularVelocityRadians,
                tweenLerp);

            _thetaRadians += _angularVelocityRadians * deltaTime;
        }

        /// <summary>
        /// Updates the position
        /// </summary>
        private void UpdatePosition(float deltaTime)
        {
            var eyePosition = _intention.Origin;
            var eyeDirection = _intention.Forward;

            UpdateCursorDistance(
                deltaTime,
                eyePosition);

            var cursorPosition = eyePosition + _cursorDistance * eyeDirection;

            var interactive = _intention.Focus;
            if (_aim > Mathf.Epsilon)
            {
                var aimMagnet = _config.GetMagnetFromAim(_aim);

                cursorPosition = Vec3.Lerp(
                    cursorPosition,
                    interactive.Focus,
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
            var targetFocusDistance = _config.GetDefaultDistanceForCursor();

            var interactive = _intention.Focus;
            if (interactive != null)
            {
                // focus on the focus widget
                var pos = interactive.Focus;
                var eyeDeltaToFocusWidget = pos - eyePosition;
                targetFocusDistance = eyeDeltaToFocusWidget.Magnitude;
            }

            var tweenDuration = _tweens.DurationSeconds(
                interactive != null
                    ? TweenIn
                    : TweenOut);

            var tweenLerp = tweenDuration > Mathf.Epsilon
                ? deltaTime / tweenDuration
                : 1.0f;

            _cursorDistance = Mathf.Lerp(
                _cursorDistance,
                targetFocusDistance,
                tweenLerp);
        }

        /// <summary>
        /// Updates the spread of the cursor
        /// </summary>
        private void UpdateAim()
        {
            _aim = -1.0f;

            var interactive = _intention.Focus;
            if (null != interactive)
            {
                _aim = interactive.Aim;
            }

            var buttonScale = interactive != null
                ? interactive.FocusScale.x
                : 1.0f;

            _spread = _config.GetReticleSpreadFromAim(_aim) * buttonScale;

            LocalColor = _config.GetReticleColorFromAim(_aim);

            _scale = _config.GetReticleScaleFromAim(_aim) * buttonScale;
        }

        /// <summary>
        /// Updates the renderer
        /// </summary>
        private void UpdateReticle(float deltaTime)
        {
            var isAiming = _intention.Focus != null;
            var tweenDuration = _tweens.DurationSeconds(
                isAiming
                    ? TweenType.Instant
                    : TweenOut);

            var tweenLerp = tweenDuration > Mathf.Epsilon
                ? deltaTime / tweenDuration
                : 1.0f;

            _reticleCenterAlpha = Mathf.Lerp(
                _reticleCenterAlpha,
                isAiming ? 0.0f : 1.0f,
                tweenLerp);

            _reticle.Update(_thetaRadians, _spread, _scale, _reticleCenterAlpha);
        }
    }
}
