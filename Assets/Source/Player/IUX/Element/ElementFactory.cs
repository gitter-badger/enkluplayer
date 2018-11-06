using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Messaging;
using CreateAR.EnkluPlayer.Assets;
using CreateAR.EnkluPlayer.Qr;
using CreateAR.EnkluPlayer.Scripting;
using CreateAR.EnkluPlayer.Vine;
using UnityEngine;

namespace CreateAR.EnkluPlayer.IUX
{
    /// <summary>
    /// <c>IElementFactory</c> implementation.
    /// </summary>
    public class ElementFactory : IElementFactory
    {
        /// <summary>
        /// Dependencies.
        /// </summary>
        private readonly IGizmoManager _gizmos;
        private readonly VineImporter _parser;
        private readonly IPrimitiveFactory _primitives;
        private readonly IIntentionManager _intention;
        private readonly IInteractionManager _interaction;
        private readonly IElementManager _elements;
        private readonly ILayerManager _layers;
        private readonly IVoiceCommandManager _voice;
        private readonly IImageLoader _imageLoader;
        private readonly IHttpService _http;
        private readonly IWorldAnchorProvider _provider;
        private readonly IScriptRequireResolver _resolver;
        private readonly IScriptManager _scripts;
        private readonly IAssetManager _assets;
        private readonly IAssetPoolManager _pools;
        private readonly IQrReaderService _qr;
        private readonly IScanLoader _scanLoader;
        private readonly IScanImporter _scanImporter;
        private readonly IMetricsService _metrics;
        private readonly IMessageRouter _messages;
        private readonly IElementJsCache _jsCache;
        private readonly IElementJsFactory _elementJsFactory;
        private readonly ColorConfig _colors;
        private readonly TweenConfig _tweens;
        private readonly WidgetConfig _config;
        private readonly ApplicationConfig _appConfig;

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
        [Construct]
        public ElementFactory(
            IElementManager elements,
            IGizmoManager gizmos,
            VineImporter parser,
            IPrimitiveFactory primitives,
            IIntentionManager intention,
            IInteractionManager interaction,
            ILayerManager layers,
            IVoiceCommandManager voice,
            IImageLoader imageLoader,
            IHttpService http,
            IWorldAnchorProvider provider,
            IScriptRequireResolver resolver,
            IScriptManager scripts,
            IAssetManager assets,
            IAssetPoolManager pools,
            IQrReaderService qr,
            IScanLoader scanLoader,
            IScanImporter scanImporter,
            IMetricsService metrics,
            IMessageRouter messages,
            IElementJsCache jsCache,
            IElementJsFactory elementJsFactory,
            ColorConfig colors,
            TweenConfig tweens,
            WidgetConfig config,
            ApplicationConfig appConfig)
        {
            _parser = parser;
            _gizmos = gizmos;
            _primitives = primitives;
            _intention = intention;
            _interaction = interaction;
            _elements = elements;
            _layers = layers;
            _colors = colors;
            _tweens = tweens;
            _voice = voice;
            _config = config;
            _imageLoader = imageLoader;
            _http = http;
            _provider = provider;
            _resolver = resolver;
            _scripts = scripts;
            _assets = assets;
            _pools = pools;
            _qr = qr;
            _scanLoader = scanLoader;
            _scanImporter = scanImporter;
            _metrics = metrics;
            _messages = messages;
            _jsCache = jsCache;
            _elementJsFactory = elementJsFactory;
            _appConfig = appConfig;
            
            // TODO: Load this all from data
            _baseSchema.Set("tweenIn", TweenType.Responsive);
            _baseSchema.Set("tweenOut", TweenType.Deliberate);
            _baseSchema.Set("color", Col4.White);
            _baseSchema.Set("virtualColor", "None");
            _baseSchema.Set("colorMode", WidgetColorMode.InheritColor);
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
                    {"label.padding", 60},
                    {"icon.scale", 1f},
                    {"fill.duration.multiplier", 1f},
                    {"aim.multiplier", 1f},
                    {"stability.multiplier", 1f}
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
                    { "layout.radius", 0.8f },
                    { "layout.degrees", 25f },
                    { "divider.offset", 0f }
                },
                Ints = new Dictionary<string, int>
                {
                    { "fontSize", 80 },
                    { "header.width", 700 },
                    { "page.size", 4 },
                }
            });
            menuSchema.Inherit(_baseSchema);

            var submenuSchema = _typeSchema[ElementTypes.SUBMENU] = new ElementSchema("Base.SubMenu");
            submenuSchema.Inherit(menuSchema);

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
            imageSchema.Load(new ElementSchemaData());

            var floatSchema = _typeSchema[ElementTypes.FLOAT] = new ElementSchema("Base.Float");
            floatSchema.Load(new ElementSchemaData
            {
                Strings = new Dictionary<string, string>
                {
                    { "face", "Camera" }
                },
                Floats = new Dictionary<string, float>
                {
                    { "fov.reorient", 3.5f }
                },
                Bools = new Dictionary<string, bool>
                {
                    { "focus.visible", true }
                },
                Vectors = new Dictionary<string, Vec3>
                {
                    { "position", new Vec3(0, 0, 2) }
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

            var qrAnchorSchema = _typeSchema[ElementTypes.QR_ANCHOR] = new ElementSchema("Base.QrAnchor");
            qrAnchorSchema.Load(new ElementSchemaData
            {
                Bools = new Dictionary<string, bool>
                {
                    { "visible", false }
                }
            });

            var lightSchema = _typeSchema[ElementTypes.LIGHT] = new ElementSchema("Base.Light");
            lightSchema.Load(new ElementSchemaData());

            var screenSchema = _typeSchema[ElementTypes.SCREEN] = new ElementSchema("Base.Screen");
            screenSchema.Load(new ElementSchemaData
            {
                Floats = new Dictionary<string, float>
                {
                    { "distance", 1.2f },
                    { "stabilization", 2f },
                    { "smoothing", 15f }
                }
            });
        }

        /// <summary>
        /// Constructor for tests. Not to be used in production code.
        /// </summary>
        public ElementFactory(IElementManager elements, IGizmoManager gizmos)
        {
            _elements = elements;
            _gizmos = gizmos;
        }

        /// <inheritdoc />
        public Element Element(ElementDescription description)
        {
            return Element(description.Collapsed());
        }

        /// <inheritdoc />
        public Element Element(string vine)
        {
            return Element(_parser.ParseSync(vine));
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
                element.Load(data, schema, children);
                _elements.Add(element);
            }

            _gizmos.Track(element);

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
                    return new ContainerWidget(new GameObject("Element"), _layers, _tweens, _colors);
                }
                case ElementTypes.IMAGE:
                {
                    return new ImageWidget(UnityEngine.Object.Instantiate(_config.Image), _layers, _tweens, _colors, _imageLoader);
                }
                case ElementTypes.CAPTION:
                {
                    return new CaptionWidget(new GameObject("Element"), _primitives, _layers, _tweens, _colors);
                }
                case ElementTypes.BUTTON:
                {
                    return new ButtonWidget(new GameObject("Element"), _config, _primitives, _layers, _tweens, _colors, _voice, _imageLoader);
                }
                case ElementTypes.CURSOR:
                {
                    return new Cursor(new GameObject("Element"), _config, _layers, _tweens, _colors, _intention, _interaction, _primitives, _appConfig.Cursor, _appConfig.Play);
                }
                case ElementTypes.MENU:
                {
                    return new MenuWidget(new GameObject("Element"), _config, _layers, _tweens, _colors, _primitives, this);
                }
                case ElementTypes.SUBMENU:
                {
                    return new SubMenuWidget(new GameObject("Element"), _layers, _tweens, _colors, this);
                }
                case ElementTypes.TEXTCRAWL:
                {
                    return new TextCrawlWidget(new GameObject("Element"), _config, _layers, _tweens, _colors, _primitives);
                }
                case ElementTypes.FLOAT:
                {
                    return new FloatWidget(new GameObject("Element"), _config, _intention, _layers, _tweens, _colors);
                }
                case ElementTypes.SCREEN:
                {
                    return new ScreenWidget(new GameObject("Element"), _layers, _tweens, _colors, _intention);
                }
                case ElementTypes.TOGGLE:
                {
                    return new ToggleWidget(new GameObject("Element"), _config, _layers, _tweens, _colors, _primitives, _voice, _imageLoader);
                }
                case ElementTypes.SLIDER:
                {
                    return new SliderWidget(new GameObject("Element"), _layers, _tweens, _colors, _interaction, this, _intention);
                }
                case ElementTypes.SELECT:
                {
                    return new SelectWidget(new GameObject("Element"), _config, _layers, _tweens, _colors, _primitives);
                }
                case ElementTypes.GRID:
                {
                    return new GridWidget(new GameObject("Element"), _config, _layers, _tweens, _colors, this);
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
                    return new ContentWidget(
                        new GameObject("Content"),
                        _layers,
                        _tweens,
                        _colors,
                        new AssetAssembler(_assets, _appConfig.Play),
                        _resolver,
                        _scripts,
                        _jsCache,
                        _elementJsFactory);
                }
                case ElementTypes.TRANSITION:
                {
                    return new Transition(new GameObject("Transition"), _tweens);
                }
                case ElementTypes.TRANSITION_SCALE:
                {
                    return new ScaleTransition(new GameObject("ScaleTransition"), _tweens);
                }
                case ElementTypes.WORLD_ANCHOR:
                {
                    return new WorldAnchorWidget(
                        new GameObject("WorldAnchor"), 
                        _layers, 
                        _tweens, 
                        _colors, 
                        _http, 
                        _provider, 
                        _metrics, 
                        _messages, 
                        _appConfig);
                }
                case ElementTypes.QR_ANCHOR:
                {
                    return new QrAnchorWidget(
                        new GameObject("QrAnchor"),
                        _layers,
                        _tweens,
                        _colors,
                        _qr,
                        _intention,
                        _elements);
                }
                case ElementTypes.LIGHT:
                {
                    return new LightWidget(
                        new GameObject("Light"),
                        _layers,
                        _tweens,
                        _colors);
                }
                case ElementTypes.SCAN:
                {
                    return new ScanWidget(
                        new GameObject("Scan"),
                        _layers,
                        _tweens,
                        _colors,
                        _scanImporter,
                        _scanLoader);
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