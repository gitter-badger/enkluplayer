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
        private readonly IInteractableManager _interactables;
        private readonly IInteractionManager _interaction;
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
            IInteractableManager interactables,
            IInteractionManager interaction,
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
            _interactables = interactables;
            _interaction = interaction;
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
            _renderer.Initialize(this, _config, Layers, Tweens, Colors, Messages, _intention, _interaction, _interactables);
        }

        /// <inheritdoc cref="Element"/>
        protected override void UnloadInternal()
        {
            Object.Destroy(_renderer.gameObject);

            base.UnloadInternal();
        }
    }
}
