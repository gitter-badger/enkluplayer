using CreateAR.Commons.Unity.Messaging;

namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// Implementation for creating primitives.
    /// </summary>
    public class PrimitiveFactory : IPrimitiveFactory
    {
        /// <summary>
        /// Dependencies.
        /// </summary>
        private readonly IElementManager _elements;
        private readonly ILayerManager _layers;
        private readonly IColorConfig _colors;
        private readonly ITweenConfig _tweens;
        private readonly IMessageRouter _messages;
        private readonly IIntentionManager _intention;
        private readonly IInteractionManager _interactions;
        private readonly IAssetPoolManager _pools;
        private readonly IInteractableManager _interactables;
        private readonly WidgetConfig _config;

        /// <summary>
        /// Constructor.
        /// </summary>
        public PrimitiveFactory(
            IElementManager elements,
            ILayerManager layers,
            IColorConfig colors,
            ITweenConfig tweens,
            IMessageRouter messages,
            IIntentionManager intention,
            IInteractionManager interactions,
            IAssetPoolManager pools,
            IInteractableManager interactables,
            WidgetConfig config)
        {
            _elements = elements;
            _layers = layers;
            _colors = colors;
            _tweens = tweens;
            _messages = messages;
            _intention = intention;
            _interactions = interactions;
            _pools = pools;
            _interactables = interactables;
            _config = config;
        }

        /// <inheritdoc cref="IPrimitiveFactory"/>
        public TextPrimitive Text()
        {
            return new TextPrimitive(_config, _pools);
        }

        /// <inheritdoc cref="IPrimitiveFactory"/>
        public ActivatorPrimitive Activator()
        {
            var activator = new ActivatorPrimitive(
                _config,
                _interactables,
                _interactions,
                _intention,
                _messages,
                _layers,
                _tweens,
                _colors);

            _elements.Add(activator);

            return activator;
        }

        /// <inheritdoc cref="IPrimitiveFactory"/>
        public ReticlePrimitive Reticle()
        {
            return new ReticlePrimitive(_config);
        }
    }
}