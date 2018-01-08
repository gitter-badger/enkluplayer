using CreateAR.Commons.Unity.Messaging;
using UnityEngine;

namespace CreateAR.SpirePlayer.IUX
{
    public class FloatPrimitive : Widget
    {
        /// <summary>
        /// Configuration.
        /// </summary>
        private readonly WidgetConfig _config;

        /// <summary>
        /// Dependencies.
        /// </summary>
        private readonly IIntentionManager _intention;

        /// <summary>
        /// Renders the float.
        /// </summary>
        private FloatRenderer _renderer;

        /// <summary>
        /// Props.
        /// </summary>
        private ElementSchemaProp<Vec3> _propOffset;

        /// <summary>
        /// Constructor.
        /// </summary>
        public FloatPrimitive(
            WidgetConfig config,
            IIntentionManager intention,
            IMessageRouter messages,
            ILayerManager layers,
            ITweenConfig tweens,
            IColorConfig colors)
            : base(
                new GameObject("Float"),
                config,
                layers,
                tweens,
                colors,
                messages)
        {
            _config = config;
            _intention = intention;
        }

        /// <inheritdoc cref="Element"/>
        protected override void LoadInternal()
        {
            base.LoadInternal();

            _renderer = Object.Instantiate(
                _config.Float,
                Vector3.zero,
                Quaternion.identity);
            _renderer.transform.SetParent(GameObject.transform, false);
            _renderer.Initialize(this, _intention);

            // load font setup.
            _propOffset = Schema.GetOwn("offset", new Vec3(0,0,2.5f));
            _propOffset.OnChanged += Offset_OnChanged;
            _renderer.Offset = _propOffset.Value.ToVector();
        }

        /// <inheritdoc cref="Element"/>
        protected override void UnloadInternal()
        {
            Object.Destroy(_renderer.gameObject);

            base.UnloadInternal();
        }

        /// <summary>
        /// Called when the label has been updated.
        /// </summary>
        /// <param name="prop">Alignment prop.</param>
        /// <param name="prev">Previous value.</param>
        /// <param name="next">Next value.</param>
        private void Offset_OnChanged(
            ElementSchemaProp<Vec3> prop,
            Vec3 prev,
            Vec3 next)
        {
            _renderer.Offset = next.ToVector();
        }
    }
}
