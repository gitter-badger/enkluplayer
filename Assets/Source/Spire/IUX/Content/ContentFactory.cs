using CreateAR.Commons.Unity.Logging;
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
        private readonly ColorConfig _colors;
        private readonly TweenConfig _tweens;
        
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
            ColorConfig colors,
            TweenConfig tweens)
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
        }

        /// <inheritdoc cref="IContentFactory"/>
        public ContentWidget Instance(IContentManager content, ContentData data)
        {
            Log.Info(this, "New content from {0}.", data);

            var assembler = new ModelContentAssembler(
                _appData,
                _assets,
                _pools,
                _progress);
            var instance = new ContentWidget(_layers,
                _tweens, 
                _colors,
                _scripts,
                assembler,
                _appData);

            var schema = new ElementSchema();
            schema.Set("src", data.Id);

            instance.Load(
                new ElementData
                {
                    Id = data.Id
                }, 
                schema,
                new Element[0]);
            
            // setup the Anchor
            var frame = _frames.Instance(content, data.Anchor.Type);
            var anchor = instance.GameObject.AddComponent<Anchor>();
            anchor.Initialize(frame, data.Anchor);

            return instance;
        }
    }
}