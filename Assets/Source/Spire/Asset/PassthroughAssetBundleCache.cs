using System;
using CreateAR.Commons.Unity.Async;
using UnityEngine;

namespace CreateAR.SpirePlayer.Assets
{
    /// <summary>
    /// No-op cache for platforms that do not support writing to disk.
    /// </summary>
    public class PassthroughAssetBundleCache : IAssetBundleCache
    {
        /// <inheritdoc />
        public void Initialize()
        {
            // no-op
        }

        /// <inheritdoc />
        public bool Contains(string uri)
        {
            return false;
        }

        /// <inheritdoc />
        public IAsyncToken<AssetBundle> Load(string uri, out LoadProgress progress)
        {
            progress = new LoadProgress
            {
                Value = 1
            };

            return new AsyncToken<AssetBundle>(new NotImplementedException());
        }

        /// <inheritdoc />
        public void Save(string uri, byte[] bytes)
        {
            // no-op
        }
    }
}