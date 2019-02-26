using CreateAR.Commons.Unity.Messaging;
using Enklu.Data;

namespace CreateAR.EnkluPlayer.IUX
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
        private readonly ColorConfig _colors;
        private readonly TweenConfig _tweens;
        private readonly IMessageRouter _messages;
        private readonly IIntentionManager _intention;
        private readonly IInteractionManager _interactions;
        private readonly WidgetConfig _config;

        /// <summary>
        /// Constructor.
        /// </summary>
        public PrimitiveFactory(
            IElementManager elements,
            ILayerManager layers,
            ColorConfig colors,
            TweenConfig tweens,
            IMessageRouter messages,
            IIntentionManager intention,
            IInteractionManager interactions,
            WidgetConfig config)
        {
            _elements = elements;
            _layers = layers;
            _colors = colors;
            _tweens = tweens;
            _messages = messages;
            _intention = intention;
            _interactions = interactions;
            _config = config;
        }

        /// <inheritdoc cref="IPrimitiveFactory"/>
        public TextPrimitive Text(ElementSchema schema)
        {
            var textPrimitive = new TextPrimitive(
                _config,
                _layers,
                _tweens,
                _colors);

            var textSchema = new ElementSchema("TextPrimitive");
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
        public ActivatorPrimitive Activator(ElementSchema schema, Widget target)
        {
            var activator = new ActivatorPrimitive(
                _config,
                _interactions,
                _intention,
                _messages,
                _layers,
                _tweens,
                _colors,
                target);

            var activatorSchema = new ElementSchema("ActivatorPrimitive");
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
    }
}