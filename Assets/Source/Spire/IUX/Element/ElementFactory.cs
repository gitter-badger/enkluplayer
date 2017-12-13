using System.Collections.Generic;
using CreateAR.Commons.Unity.Messaging;
using Vuforia;

namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// <c>IElementFactory</c> implementation.
    /// </summary>
    public class ElementFactory : IElementFactory
    {
        /// <summary>
        /// Dependencies.
        /// </summary>
        private readonly IPrimitiveFactory _primitives;
        private readonly IIntentionManager _intention;
        private readonly IElementManager _elements;
        private readonly ILayerManager _layers;
        private readonly IColorConfig _colors;
        private readonly ITweenConfig _tweens;
        private readonly IMessageRouter _messages;
        private readonly WidgetConfig _config;

        /// <summary>
        /// All widgets inherit this base schema
        /// </summary>
        private readonly ElementSchema _baseSchema = new ElementSchema();

        private readonly Dictionary<int, ElementSchema> _typeSchema = new Dictionary<int, ElementSchema>();

        /// <summary>
        /// Constructor.
        /// </summary>
        public ElementFactory(
            IPrimitiveFactory primitives,
            IIntentionManager intention,
            IElementManager elements,
            ILayerManager layers,
            IColorConfig colors,
            ITweenConfig tweens,
            IMessageRouter messages,
            WidgetConfig config)
        {
            _primitives = primitives;
            _intention = intention;
            _elements = elements;
            _layers = layers;
            _colors = colors;
            _tweens = tweens;
            _messages = messages;
            _config = config;

            // TODO: Load this all from data
            _baseSchema.Set("tweenIn", TweenType.Responsive);
            _baseSchema.Set("tweenOut", TweenType.Deliberate);
            _baseSchema.Set("color", Col4.White);
            _baseSchema.Set("virtualColor", VirtualColor.None);
            _baseSchema.Set("colorMode", ColorMode.InheritColor);
            _baseSchema.Set("visibilityMode", VisibilityMode.Inherit);
            _baseSchema.Set("layerMode", LayerMode.Default);
            _baseSchema.Set("autoDestroy", false);

            // load defaults
            var buttonSchema = _typeSchema[ElementTypes.BUTTON] = new ElementSchema();
            buttonSchema.Load(new ElementSchemaData
            {
                Ints = new Dictionary<string, int>
                {
                    {"ready.frameColor", (int) VirtualColor.Ready},
                    {"ready.captionColor", (int) VirtualColor.Primary},
                    {"ready.tween", (int) TweenType.Responsive},

                    {"activating.frameColor", (int) VirtualColor.Interacting},
                    {"activating.captionColor", (int) VirtualColor.Interacting},
                    {"activating.tween", (int) TweenType.Responsive},

                    {"activated.color", (int) VirtualColor.Interacting},
                    {"activated.captionColor", (int) VirtualColor.Interacting},
                    {"activated.tween", (int) TweenType.Instant},
                },
                Floats = new Dictionary<string, float>
                {
                    {"ready.frameScale", 1.0f},
                    {"activating.frameScale", 1.1f},
                    {"activated.frameScale", 1.0f},
                }
            });
            buttonSchema.Wrap(_baseSchema);
        }

        /// <inheritdoc cref="IElementFactory"/>
        public Element Element(ElementDescription description)
        {
            return Element(description.Collapsed());
        }
        
        /// <summary>
        /// Recursive method that creates an <c>Element</c> from data.
        /// </summary>
        /// <param name="data">Data to create the element from.</param>
        /// <returns></returns>
        private Element Element(ElementData data)
        {
            // children first
            var childData = data.Children;
            var childDataLen = childData.Length;
            var children = new Element[childDataLen];
            for (int i = 0, len = childData.Length; i < len; i++)
            {
                children[i] = Element(childData[i]);
            }
            
            // element
            var schema = new ElementSchema();
            schema.Load(data.Schema);

            // find appropriate parent schema
            ElementSchema parentSchema;
            if (!_typeSchema.TryGetValue(data.Type, out parentSchema))
            {
                parentSchema = _baseSchema;
            }
            schema.Wrap(parentSchema);

            var element = ElementForType(data.Type);
            if (element != null)
            {
                _elements.Add(element);
                element.Load(data, schema, children);
            }

            return element;
        }

        /// <summary>
        /// Creates an element of the type specified.
        /// </summary>
        /// <param name="type">The type of element.</param>
        /// <returns></returns>
        private Element ElementForType(int type)
        {
            switch (type)
            {
                case ElementTypes.CONTAINER:
                {
                    return new Element();
                }
                case ElementTypes.CAPTION:
                {
                    return new Caption(_config, _primitives, _layers, _tweens, _colors, _messages);
                }
                case ElementTypes.BUTTON:
                {
                    return new Button(_config, _primitives, _layers, _tweens, _colors, _messages);
                }
                case ElementTypes.CURSOR:
                {
                    return new Cursor(_config, _primitives, _layers, _tweens, _colors, _messages, _intention);
                }
                default:
                {
                    return new Element();
                }
            }
        }
    }
}