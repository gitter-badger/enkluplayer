using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.Assets;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Crates content.
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

        /// <summary>
        /// Constructor.
        /// </summary>
        public ContentFactory(
            IAppDataManager appData,
            IAssetManager assets,
            IScriptManager scripts,
            IAssetPoolManager pools,
            IAnchorReferenceFrameFactory frames)
        {
            _appData = appData;
            _assets = assets;
            _scripts = scripts;
            _pools = pools;
            _frames = frames;
        }

        /// <inheritdoc cref="IContentFactory"/>
        public Content Instance(IContentManager content, ContentData data)
        {
            Log.Info(this, "New content from {0}.", data);

            // TODO: Refactor Content
            return null;
            /*
            var instance = new GameObject(data.Name);

            // setup the Anchor
            var frame = _frames.Instance(content, data.Anchor.Type);
            var anchor = instance.AddComponent<Anchor>();
            anchor.Initialize(frame, data.Anchor);

            // setup the content
            var newContent = instance.AddComponent<Content>();
            newContent.Setup(_appData, _assets, _scripts, _pools, data);

            return newContent;
            */
        }
    }
}