using System;
using CreateAR.Commons.Unity.Async;
using UnityEngine;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// Implementation for ArKit.
    /// </summary>
    public class ArKitWorldAnchorProvider : IWorldAnchorProvider
    {
        /// <inheritdoc />
        public bool IsImporting { get; }

        /// <inheritdoc />
        public IAsyncToken<Void> Initialize(IAppSceneManager scenes)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public IAsyncToken<Void> Anchor(string id, GameObject gameObject)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void UnAnchor(GameObject gameObject)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void ClearAllAnchors()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public IAsyncToken<byte[]> Export(string id, GameObject gameObject)
        {
            return new AsyncToken<byte[]>(new NotImplementedException());
        }

        /// <inheritdoc />
        public IAsyncToken<Void> Import(string id, byte[] bytes, GameObject gameObject)
        {
            return new AsyncToken<Void>(new NotImplementedException());
        }
        
    }
}