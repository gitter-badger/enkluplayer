using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Messaging;
using CreateAR.EnkluPlayer.Assets;
using CreateAR.EnkluPlayer.Qr;
using CreateAR.EnkluPlayer.Scripting;
using CreateAR.EnkluPlayer.Vine;
using Source.Player.IUX;
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
        private readonly IQrReaderService _qr;
        private readonly IScanLoader _scanLoader;
        private readonly IScanImporter _scanImporter;
        private readonly IMetricsService _metrics;
        private readonly IMessageRouter _messages;
        private readonly IElementJsCache _jsCache;
        private readonly IBootstrapper _bootstrapper;
        private readonly ColorConfig _colors;
        private readonly TweenConfig _tweens;
        private readonly WidgetConfig _config;
        private readonly ApplicationConfig _appConfig;
        private readonly ElementSchemaDefaults _elementSchemaDefaults;

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
            IBootstrapper bootstrapper,
            ColorConfig colors,
            TweenConfig tweens,
            WidgetConfig config,
            ApplicationConfig appConfig,
            EditorSettings editorSettings,
            ElementSchemaDefaults elementSchemaDefaults)
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
            _qr = qr;
            _scanLoader = scanLoader;
            _scanImporter = scanImporter;
            _metrics = metrics;
            _messages = messages;
            _jsCache = jsCache;
            _bootstrapper = bootstrapper;
            _appConfig = appConfig;
            _elementSchemaDefaults = elementSchemaDefaults;
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
            var parentSchema = _elementSchemaDefaults.Get(data.Type);
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
                    return new TextWidget(new GameObject("Element"), _primitives, _layers, _tweens, _colors);
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
                    return new SliderWidget(new GameObject("Element"), _layers, _tweens, _colors, this, _intention);
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
                        _jsCache);
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
                        _bootstrapper,
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
