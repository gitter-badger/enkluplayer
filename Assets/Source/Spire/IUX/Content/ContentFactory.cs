using CreateAR.SpirePlayer;
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
        private readonly IAssetManager _assets;
        private readonly IScriptManager _scripts;
        private readonly IAnchorReferenceFrameFactory _frames;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ContentFactory(
            IAssetManager assets,
            IScriptManager scripts,
            IAnchorReferenceFrameFactory frames)
        {
            _assets = assets;
            _scripts = scripts;
            _frames = frames;
        }

        /// <inheritdoc cref="IContentFactory"/>
        public Content Instance(IContentManager content, ContentData data)
        {
            var instance = new GameObject(data.Name);

            // setup the Anchor
            var frame = _frames.Instance(content, data.Anchor.Type);
            var anchor = instance.AddComponent<Anchor>();
            anchor.Initialize(frame, data.Anchor);

            // setup the content
            var newContent = instance.AddComponent<Content>();
            newContent.Setup(_assets, _scripts, data);

            return newContent;
        }
    }
}