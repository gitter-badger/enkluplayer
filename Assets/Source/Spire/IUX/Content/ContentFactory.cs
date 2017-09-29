﻿using CreateAR.SpirePlayer;
using UnityEngine;

namespace CreateAR.Spire
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
        private readonly IAnchorReferenceFrameFactory _frames;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ContentFactory(
            IAssetManager assets,
            IAnchorReferenceFrameFactory frames)
        {
            _assets = assets;
            _frames = frames;
        }

        /// <inheritdoc cref="IContentFactory"/>
        public Content Instance(ContentData data)
        {
            var instance = new GameObject(data.Name);

            // setup the Anchor
            var anchor = instance.AddComponent<Anchor>();
            var frame = _frames.Instance(anchor);
            anchor.Initialize(frame, data.Anchor);

            // setup the content
            var content = instance.AddComponent<Content>();
            content.Setup(_assets, data);

            return content;
        }
    }
}