using UnityEngine;

namespace CreateAR.EnkluPlayer.IUX
{
    /// <summary>
    /// IUX widget that locks children to screen.
    /// </summary>
    public class ScreenWidget : Widget
    {
        /// <summary>
        /// Intention.
        /// </summary>
        private readonly IIntentionManager _intention;

        /// <summary>
        /// Distance from camera.
        /// </summary>
        private ElementSchemaProp<float> _distanceProp;

        /// <summary>
        /// How much directional drift to ignore
        /// </summary>
        private ElementSchemaProp<float> _stabilizationProp;

        /// <summary>
        /// How fast (deg/sec) to update the position
        /// </summary>
        private ElementSchemaProp<float> _smoothingProp;

        /// <summary>
        /// A stable position near the intent's direction.
        /// </summary>
        private Vec3 _stabilizedForward;

        /// <summary>
        /// The previous stable position.
        /// </summary>
        private Vec3 _lastStableForward;

        /// <summary>
        /// The current forward, a value between stable & last stable.
        /// </summary>
        private Vec3 _interpolatedForward;

        /// <summary>
        /// The angular distance between stable/last.
        /// </summary>
        private float _lerpDist = 0;

        /// <summary>
        /// The current transition progress between stable/last.
        /// </summary>
        private float _lerpAcc = 0;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ScreenWidget(
            GameObject gameObject,
            ILayerManager layers,
            TweenConfig tweens,
            ColorConfig colors,
            IIntentionManager intention)
            : base(gameObject, layers, tweens, colors)
        {
            _intention = intention;

            _interpolatedForward = _stabilizedForward = _intention.Forward;
        }

        /// <inheritdoc />
        protected override void LoadInternalAfterChildren()
        {
            base.LoadInternalAfterChildren();

            _distanceProp = Schema.GetOwn("distance", 1f);
            _stabilizationProp = Schema.GetOwn("stabilization", 2f);
            _smoothingProp = Schema.GetOwn("smoothing", 15f);
        }

        /// <inheritdoc />
        protected override void UpdateInternal()
        {
            base.UpdateInternal();

            // Update stabilized forward as needed
            if (Vec3.Angle(_stabilizedForward, _intention.Forward) > _stabilizationProp.Value)
            {
                _lastStableForward = _interpolatedForward;
                _stabilizedForward = _intention.Forward;

                _lerpDist = Vec3.Angle(_stabilizedForward, _interpolatedForward);
                _lerpAcc = 0;
            }
            
            // Update the interpolated forward as needed
            if (_lerpAcc < 1)
            {
                var step = (_smoothingProp.Value * Time.deltaTime) / _lerpDist;
                _lerpAcc = Mathf.Min(1, _lerpAcc + step);
                _interpolatedForward = Vec3.Lerp(_lastStableForward, _stabilizedForward, _lerpAcc).Normalized;
            }

            var pos = _intention.Origin + _distanceProp.Value * _interpolatedForward;
            GameObject.transform.position = pos.ToVector();
            GameObject.transform.forward = _interpolatedForward.ToVector();
        }
    }
}