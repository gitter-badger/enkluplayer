using CreateAR.Commons.Unity.Messaging;

namespace CreateAR.SpirePlayer.UI
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
        private readonly IWidgetConfig _config;
        private readonly ITweenConfig _tweens;
        private readonly IMessageRouter _messages;
        private readonly IInteractionManager _interactions;

        /// <summary>
        /// All widgets inherit this base schema
        /// </summary>
        private ElementSchema _baseSchema = new ElementSchema();

        /// <summary>
        /// Constructor.
        /// </summary>
        public ElementFactory(
            IPrimitiveFactory primitives,
            IIntentionManager intention,
            IElementManager elements,
            ILayerManager layers,
            IColorConfig colors,
            IWidgetConfig config,
            ITweenConfig tweens,
            IMessageRouter messages,
            IInteractionManager interactions)
        {
            _primitives = primitives;
            _intention = intention;
            _elements = elements;
            _layers = layers;
            _colors = colors;
            _config = config;
            _tweens = tweens;
            _messages = messages;
            _interactions = interactions;

            /// TODO: Load this all from data
            _baseSchema.Set("tweenIn", TweenType.Responsive);
            _baseSchema.Set("tweenOut", TweenType.Deliberate);
            _baseSchema.Set("color", Col4.White);
            _baseSchema.Set("virtualColor", VirtualColor.None);
            _baseSchema.Set("colorMode", ColorMode.InheritColor);
            _baseSchema.Set("visibilityMode", VisibilityMode.Inherit);
            _baseSchema.Set("layerMode", LayerMode.Default);
            _baseSchema.Set("autoDestroy", false);
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
            schema.Wrap(_baseSchema);

            var element = Element(data.Schema);
            if (element != null)
            {
                _elements.Add(element);
                element.Load(data, schema, children);
            }

            return element;
        }

        /// <summary>
        /// Creates an element of the type corresponding to the type in the schema.
        /// </summary>
        /// <param name="schemaData"></param>
        /// <returns></returns>
        private Element Element(ElementSchemaData schemaData)
        {
            if (null != schemaData.Ints)
            {
                int elementType;
                if (schemaData.Ints.TryGetValue("type", out elementType))
                {
                    Element newElement = null;
                    switch (elementType)
                    {
                        case ElementTypes.CAPTION:
                            var newCaption = new Caption();
                            newCaption.Initialize(_config, _layers, _tweens, _colors, _primitives, _messages);
                            newElement = newCaption;
                            break;

                        case ElementTypes.BUTTON:
                            var newButton = new Button();
                            newButton.Initialize(_config, _layers, _tweens, _colors, _primitives, _messages, _intention, _interactions);
                            newElement = newButton;
                            break;

                        case ElementTypes.BUTTON_READY_STATE:
                            newElement = new ButtonReadyState();
                            break;

                        case ElementTypes.BUTTON_ACTIVATING_STATE:
                            newElement = new ButtonActivatingState();
                            break;

                        case ElementTypes.BUTTON_ACTIVATED_STATE:
                            newElement = new ButtonActivatedState();
                            break;

                        case ElementTypes.CURSOR:
                            var newCursor = new Cursor();
                            newCursor.Initialize(_config, _layers, _tweens, _colors, _primitives, _messages, _intention);
                            newElement = newCursor;
                            break;
                    }

                    return newElement;
                }
            }

            return new Element();
        }
    }
}