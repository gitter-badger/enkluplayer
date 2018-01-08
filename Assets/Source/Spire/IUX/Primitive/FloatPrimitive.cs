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
                new GameObject("Text"),
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
        }

        /// <inheritdoc cref="Element"/>
        protected override void UnloadInternal()
        {
            Object.Destroy(_renderer.gameObject);

            base.UnloadInternal();
        }
    }
}
