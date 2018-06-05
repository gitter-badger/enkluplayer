﻿using System;
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
        public IAsyncToken<byte[]> Export(string id, GameObject gameObject)
        {
            return new AsyncToken<byte[]>(new NotImplementedException());
        }

        /// <inheritdoc />
        public IAsyncToken<Void> Import(string id, GameObject gameObject, byte[] bytes)
        {
            return new AsyncToken<Void>(new NotImplementedException());
        }

        /// <inheritdoc />
        public void Disable(GameObject gameObject)
        {
            throw new NotImplementedException();
        }
    }
}