using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using CreateAR.SpirePlayer.Assets;
using CreateAR.SpirePlayer.IUX;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Creates instances of <c>Content</c>.
    /// </summary>
    public class ContentFactory : IContentFactory
    {
        /// <summary>
        /// Dependencies.
        /// </summary>
        private readonly IAppDataManager _appData;
        private readonly IAssetManager _assets;
        private readonly IScriptManager _scripts;
        private readonly IAssetPoolManager _pools;
        private readonly IAnchorReferenceFrameFactory _frames;
        private readonly ILoadProgressManager _progress;
        private readonly ILayerManager _layers;
        private readonly IColorConfig _colors;
        private readonly ITweenConfig _tweens;
        private readonly IMessageRouter _messages;
        private readonly WidgetConfig _config;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ContentFactory(
            IAppDataManager appData,
            IAssetManager assets,
            IScriptManager scripts,
            IAssetPoolManager pools,
            IAnchorReferenceFrameFactory frames,
            ILoadProgressManager progress,
            ILayerManager layers,
            IColorConfig colors,
            ITweenConfig tweens,
            IMessageRouter messages,
            WidgetConfig config)
        {
            _appData = appData;
            _assets = assets;
            _scripts = scripts;
            _pools = pools;
            _frames = frames;
            _progress = progress;
            _layers = layers;
            _colors = colors;
            _tweens = tweens;
            _messages = messages;
            _config = config;
        }

        /// <inheritdoc cref="IContentFactory"/>
        public Content Instance(IContentManager content, ContentData data)
        {
            Log.Info(this, "New content from {0}.", data);

            var assembler = new ModelContentAssembler(
                _appData,
                _assets,
                _pools,
                _progress);
            var instance = new Content(
                _config,
                _layers,
                _tweens, 
                _colors,
                _messages,
                _scripts,
                assembler);

            // TODO: Move ContentData into Schema and use Element creation flow
            instance.Load(
                new ElementData
                {
                    Id = data.Id
                }, 
                new ElementSchema(),
                new Element[0]);
            instance.Setup(data);

            // setup the Anchor
            var frame = _frames.Instance(content, data.Anchor.Type);
            var anchor = instance.GameObject.AddComponent<Anchor>();
            anchor.Initialize(frame, data.Anchor);

            return instance;
        }
    }
}