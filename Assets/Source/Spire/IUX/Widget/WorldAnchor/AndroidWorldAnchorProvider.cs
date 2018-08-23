using System;
using CreateAR.Commons.Unity.Async;
using UnityEngine;

namespace CreateAR.SpirePlayer.IUX
{
    public class AndroidWorldAnchorProvider : IWorldAnchorProvider
    {
        /// <inheritdoc />
        public IAsyncToken<Commons.Unity.Async.Void> Anchor(string id, GameObject gameObject)
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
        public IAsyncToken<Commons.Unity.Async.Void> Import(string id, byte[] bytes, GameObject gameObject)
        {
            return new AsyncToken<Commons.Unity.Async.Void>(new NotImplementedException());
        }

        /// <inheritdoc />
        public IAsyncToken<Commons.Unity.Async.Void> Initialize(IAppSceneManager scenes)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void UnAnchor(GameObject gameObject)
        {
            throw new NotImplementedException();
        }
    }
}
