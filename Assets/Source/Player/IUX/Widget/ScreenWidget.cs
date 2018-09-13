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
        }

        /// <inheritdoc />
        protected override void LoadInternalAfterChildren()
        {
            base.LoadInternalAfterChildren();

            _distanceProp = Schema.GetOwn("distance", 1f);
        }

        /// <inheritdoc />
        protected override void UpdateInternal()
        {
            base.UpdateInternal();

            var pos = _intention.Origin + _distanceProp.Value * _intention.Forward;
            GameObject.transform.position = pos.ToVector();
            GameObject.transform.forward = _intention.Forward.ToVector();
        }
    }
}