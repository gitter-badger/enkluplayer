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
        public TextPrimitive Text(ElementSchema schema)
        {
            var textPrimitive = new TextPrimitive(
                _config,
                _interactables,
                _interactions,
                _intention,
                _messages,
                _layers,
                _tweens,
                _colors);

            var textSchema = new ElementSchema();
            textSchema.Wrap(schema);
            textPrimitive.Load(
                new ElementData
                {
                    Id = "Text"
                },
                textSchema,
                new Element[0]);

            _elements.Add(textPrimitive);

            return textPrimitive;
        }

        /// <inheritdoc cref="IPrimitiveFactory"/>
        public ActivatorPrimitive Activator(ElementSchema schema)
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

            var activatorSchema = new ElementSchema();
            activatorSchema.Wrap(schema);
            activator.Load(
                new ElementData
                {
                    Id = "Activator"
                },
                activatorSchema,
                new Element[0]);

            _elements.Add(activator);

            return activator;
        }

        /// <inheritdoc cref="IPrimitiveFactory"/>
        public ReticlePrimitive Reticle()
        {
            return new ReticlePrimitive(_config);
        }

        /// <inheritdoc cref="IPrimitiveFactory"/>
        public FloatPrimitive Float(ElementSchema schema)
        {
            var floatPrimitive = new FloatPrimitive(
                _config,
                _intention,
                _messages,
                _layers,
                _tweens,
                _colors);

            var textSchema = new ElementSchema();
            textSchema.Wrap(schema);
            floatPrimitive.Load(
                new ElementData
                {
                    Id = "Float"
                },
                textSchema,
                new Element[0]);

            _elements.Add(floatPrimitive);

            return floatPrimitive;
        }
    }
}