using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Messaging;
using UnityEngine;

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
        private readonly ColorConfig _colors;
        private readonly TweenConfig _tweens;
        private readonly IMessageRouter _messages;
        private readonly IVoiceCommandManager _voice;
        private readonly WidgetConfig _config;

        /// <summary>
        /// All widgets inherit this base schema
        /// </summary>
        private readonly ElementSchema _baseSchema = new ElementSchema("Base");

        /// <summary>
        /// Lookup from element type to base schema for that type.
        /// </summary>
        private readonly Dictionary<int, ElementSchema> _typeSchema = new Dictionary<int, ElementSchema>();

        /// <summary>
        /// Constructor.
        /// </summary>
        public ElementFactory(
            IPrimitiveFactory primitives,
            IIntentionManager intention,
            IElementManager elements,
            ILayerManager layers,
            ColorConfig colors,
            TweenConfig tweens,
            IMessageRouter messages,
            IVoiceCommandManager voice,
            WidgetConfig config)
        {
            _primitives = primitives;
            _intention = intention;
            _elements = elements;
            _layers = layers;
            _colors = colors;
            _tweens = tweens;
            _messages = messages;
            _voice = voice;
            _config = config;

            // TODO: Load this all from data
            _baseSchema.Set("tweenIn", TweenType.Responsive);
            _baseSchema.Set("tweenOut", TweenType.Deliberate);
            _baseSchema.Set("color", Col4.White);
            _baseSchema.Set("virtualColor", "None");
            _baseSchema.Set("colorMode", WidgetColorMode.InheritColor);
            _baseSchema.Set("visibilityMode", WidgetVisibilityMode.Inherit);
            _baseSchema.Set("layerMode", LayerMode.Default);
            _baseSchema.Set("autoDestroy", false);
            _baseSchema.Set("font", "Watchword_bold");

            // load defaults
            var buttonSchema = _typeSchema[ElementTypes.BUTTON] = new ElementSchema("Base.Button");
            buttonSchema.Load(new ElementSchemaData
            {
                Ints = new Dictionary<string, int>
                {
                    { "fontSize", 80 },

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

                    {"label.padding", 0.05f},
                },
                Vectors = new Dictionary<string, Vec3>
                {
                    { "position", new Vec3(0f, 0f, 0f) }
                }
            });
            buttonSchema.Inherit(_baseSchema);

            _typeSchema[ElementTypes.SELECT] = _typeSchema[ElementTypes.TOGGLE] = buttonSchema;

            var menuSchema = _typeSchema[ElementTypes.MENU] = new ElementSchema("Base.Menu");
            menuSchema.Load(new ElementSchemaData
            {
                Strings = new Dictionary<string, string>
                {
                    { "layout", "Radial" }
                },
                Floats = new Dictionary<string, float>
                {
                    { "layout.radius", 0.25f },
                    { "layout.degrees", 70f }
                },
                Ints = new Dictionary<string, int>
                {
                    { "fontSize", 80 },
                    { "header.width", 700 },
                }
            });
            menuSchema.Inherit(_baseSchema);
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
            var schema = new ElementSchema(data.Id);
            schema.Load(data.Schema);

            // find appropriate schema to inherit
            ElementSchema parentSchema;
            if (!_typeSchema.TryGetValue(data.Type, out parentSchema))
            {
                parentSchema = _baseSchema;
            }
            schema.Inherit(parentSchema);

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
            var gameObject = new GameObject("Element");

            switch (type)
            {
                case ElementTypes.CONTAINER:
                {
                    return new Container(gameObject, _config, _layers, _tweens, _colors, _messages);
                }
                case ElementTypes.CAPTION:
                {
                    return new Caption(gameObject, _config, _primitives, _layers, _tweens, _colors, _messages);
                }
                case ElementTypes.BUTTON:
                {
                    return new Button(gameObject, _config, _primitives, _layers, _tweens, _colors, _messages, _voice);
                }
                case ElementTypes.CURSOR:
                {
                    return new Cursor(gameObject, _config, _primitives, _layers, _tweens, _colors, _messages, _intention);
                }
                case ElementTypes.MENU:
                {
                    return new Menu(gameObject, _config, _layers, _tweens, _colors, _messages, _primitives, this);
                }
                case ElementTypes.TEXTCRAWL:
                {
                    return new TextCrawl(gameObject, _config, _layers, _tweens, _colors, _messages, _primitives);
                }
                case ElementTypes.FLOAT:
                {
                    return new Float(gameObject, _config, _intention, _messages, _layers, _tweens, _colors);
                }
                case ElementTypes.TOGGLE:
                {
                    return new Toggle(gameObject, _config, _layers, _tweens, _colors, _messages, _primitives, _voice);
                }
                case ElementTypes.SLIDER:
                {
                    return new Slider(gameObject, _config, _layers, _tweens, _colors, _messages, _primitives);
                }
                case ElementTypes.SELECT:
                {
                    return new Select(gameObject, _config, _layers, _tweens, _colors, _messages, _primitives);
                }
                case ElementTypes.GRID:
                {
                    return new Grid(gameObject, _config, _layers, _tweens, _colors, _messages, _primitives);
                }
                case ElementTypes.OPTION:
                {
                    return new Option(gameObject);
                }
                default:
                {
                    throw new Exception(string.Format(
                        "Invalid element type : {0}.",
                        type));
                }
            }
        }
    }
}