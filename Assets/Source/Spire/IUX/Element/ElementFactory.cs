using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Messaging;
using CreateAR.SpirePlayer.Vine;
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
        private readonly VineImporter _parser;
        private readonly IPrimitiveFactory _primitives;
        private readonly IIntentionManager _intention;
        private readonly IInteractionManager _interaction;
        private readonly IElementManager _elements;
        private readonly ILayerManager _layers;
        private readonly ColorConfig _colors;
        private readonly TweenConfig _tweens;
        private readonly IMessageRouter _messages;
        private readonly IVoiceCommandManager _voice;
        private readonly WidgetConfig _config;
        private readonly IImageLoader _imageLoader;
        private readonly IContentFactory _content;
        private readonly IContentManager _contentManager;

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
            VineImporter parser,
            IPrimitiveFactory primitives,
            IIntentionManager intention,
            IInteractionManager interaction,
            IElementManager elements,
            ILayerManager layers,
            ColorConfig colors,
            TweenConfig tweens,
            IMessageRouter messages,
            IVoiceCommandManager voice,
            WidgetConfig config,
            IImageLoader imageLoader,
            IContentFactory content,
            IContentManager contentManager)
        {
            _parser = parser;
            _primitives = primitives;
            _intention = intention;
            _interaction = interaction;
            _elements = elements;
            _layers = layers;
            _colors = colors;
            _tweens = tweens;
            _messages = messages;
            _voice = voice;
            _config = config;
            _imageLoader = imageLoader;
            _content = content;
            _contentManager = contentManager;
            
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
                    { "fontSize", 70 }
                },
                Strings = new Dictionary<string, string>
                {
                    {"ready.color", VirtualColor.Ready.ToString()},
                    {"ready.captionColor", VirtualColor.Primary.ToString()},
                    {"ready.tween", TweenType.Responsive.ToString()},

                    {"activating.color", VirtualColor.Interacting.ToString()},
                    {"activating.captionColor", VirtualColor.Interacting.ToString()},
                    {"activating.tween", TweenType.Responsive.ToString()},

                    {"activated.color", VirtualColor.Interacting.ToString()},
                    {"activated.captionColor", VirtualColor.Interacting.ToString()},
                    {"activated.tween", TweenType.Responsive.ToString()}
                },
                Floats = new Dictionary<string, float>
                {
                    {"ready.frameScale", 1.0f},
                    {"activating.frameScale", 1.1f},
                    {"activated.frameScale", 1.0f},

                    {"label.padding", 0.05f},

                    { "icon.scale", 1f }
                },
                Vectors = new Dictionary<string, Vec3>
                {
                    { "position", new Vec3(0f, 0f, 0f) },

                    { "ready.scale", new Vec3(1, 1, 1) },
                    { "activating.scale", new Vec3(1.1f, 1.1f, 1.1f) },
                    { "activated.scale", new Vec3(1, 1, 1) }
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

            var gridSchema = _typeSchema[ElementTypes.GRID] = new ElementSchema("Base.Grid");
            gridSchema.Load(new ElementSchemaData
            {
                Floats = new Dictionary<string, float>
                {
                    { "padding.vertical", .15f },
                    { "padding.horizontal", .15f }
                }
            });

            var imageSchema = _typeSchema[ElementTypes.IMAGE] = new ElementSchema("Base.Image");
            imageSchema.Load(new ElementSchemaData
            {
                Floats = new Dictionary<string, float>
                {
                    { "width", 0.1f },
                    { "height", 0.1f }
                }
            });

            var floatSchema = _typeSchema[ElementTypes.FLOAT] = new ElementSchema("Base.Float");
            floatSchema.Load(new ElementSchemaData
            {
                Floats = new Dictionary<string, float>
                {
                    { "fov.reorient", 1.5f }
                },
                Bools = new Dictionary<string, bool>
                {
                    { "focus.visible", true }
                }
            });

            var sliderSchema = _typeSchema[ElementTypes.SLIDER] = new ElementSchema("Base.Slider");
            sliderSchema.Load(new ElementSchemaData
            {
                Floats = new Dictionary<string, float>
                {
                    { "size.max", 3f },
                    { "size.min", 1f },
                    { "radius", 0.25f },
                    { "length", 0.1f },
                },
                Strings = new Dictionary<string, string>
                {
                    { "axis", "x" }
                }
            });
        }

        /// <inheritdoc />
        public Element Element(ElementDescription description)
        {
            return Element(description.Collapsed());
        }

        /// <inheritdoc />
        public Element Element(string vine)
        {
            return Element(_parser.Parse(vine));
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
            switch (type)
            {
                case ElementTypes.CONTAINER:
                {
                    return new ContainerWidget(new GameObject("Element"), _config, _layers, _tweens, _colors, _messages);
                }
                case ElementTypes.IMAGE:
                {
                    return new ImageWidget(UnityEngine.Object.Instantiate(_config.Image), _config, _layers, _tweens, _colors, _messages, _imageLoader);
                }
                case ElementTypes.CAPTION:
                {
                    return new CaptionWidget(new GameObject("Element"), _config, _primitives, _layers, _tweens, _colors, _messages);
                }
                case ElementTypes.BUTTON:
                {
                    return new ButtonWidget(new GameObject("Element"), _config, _primitives, _layers, _tweens, _colors, _messages, _voice, _imageLoader);
                }
                case ElementTypes.CURSOR:
                {
                    return new Cursor(new GameObject("Element"), _config, _layers, _tweens, _colors, _messages, _intention, _interaction, _primitives);
                }
                case ElementTypes.MENU:
                {
                    return new MenuWidget(new GameObject("Element"), _config, _layers, _tweens, _colors, _messages, _primitives, this);
                }
                case ElementTypes.TEXTCRAWL:
                {
                    return new TextCrawlWidget(new GameObject("Element"), _config, _layers, _tweens, _colors, _messages, _primitives);
                }
                case ElementTypes.FLOAT:
                {
                    return new FloatWidget(new GameObject("Element"), _config, _intention, _messages, _layers, _tweens, _colors);
                }
                case ElementTypes.TOGGLE:
                {
                    return new ToggleWidget(new GameObject("Element"), _config, _layers, _tweens, _colors, _messages, _primitives, _voice, _imageLoader);
                }
                case ElementTypes.SLIDER:
                {
                    return new SliderWidget(new GameObject("Element"), _config, _layers, _tweens, _colors, _messages, _interaction, this, _intention);
                }
                case ElementTypes.SELECT:
                {
                    return new SelectWidget(new GameObject("Element"), _config, _layers, _tweens, _colors, _messages, _primitives);
                }
                case ElementTypes.GRID:
                {
                    return new GridWidget(new GameObject("Element"), _config, _layers, _tweens, _colors, _messages, this);
                }
                case ElementTypes.OPTION:
                {
                    return new Option();
                }
                case ElementTypes.OPTION_GROUP:
                {
                    return new OptionGroup();
                }
                case ElementTypes.CONTENT:
                {
                    return _content.Instance(_contentManager, new ContentData());
                }
                case ElementTypes.TRANSITION_SCALE:
                {
                    return new ScaleTransition(new GameObject("ScaleTransition"), _tweens);
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