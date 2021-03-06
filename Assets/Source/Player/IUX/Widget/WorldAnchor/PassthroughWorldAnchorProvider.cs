﻿using System;
using System.Collections;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using UnityEngine;
using Random = System.Random;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.EnkluPlayer.IUX
{
    /// <summary>
    /// Passthrough implementation for non-AR platforms. Generates random data
    /// with a real delay.
    /// </summary>
    public class PassthroughWorldAnchorProvider : IWorldAnchorProvider
    {
        /// <summary>
        /// PRNG.
        /// </summary>
        private readonly Random _rand = new Random();

        /// <summary>
        /// Consts.
        /// </summary>
        private const int IMPORT_DELAY_SEC = 3;
        private const int EXPORT_DELAY_SEC = 5;

        /// <summary>
        /// Bootstrapper.
        /// </summary>
        private readonly IBootstrapper _bootstrapper;

        /// <summary>
        /// Constructor.
        /// </summary>
        public PassthroughWorldAnchorProvider(IBootstrapper bootstrapper)
        {
            _bootstrapper = bootstrapper;
        }

        /// <inheritdoc />
        public void ClearAllAnchors()
        {
            //
        }

        /// <inheritdoc />
        public IAsyncToken<byte[]> Export(string id, GameObject gameObject)
        {
            var token = new AsyncToken<byte[]>();

            _bootstrapper.BootstrapCoroutine(Delay(
                EXPORT_DELAY_SEC,
                () => token.Succeed(RandomPayload())));

            return token;
        }

        /// <inheritdoc />
        public IAsyncToken<Void> Import(string id, byte[] bytes, GameObject gameObject)
        {
            var token = new AsyncToken<Void>();

            _bootstrapper.BootstrapCoroutine(Delay(
                IMPORT_DELAY_SEC,
                () => token.Succeed(Void.Instance)));

            return token;
        }

        /// <inheritdoc />
        public bool IsImporting { get; private set; }

        /// <inheritdoc />
        public IAsyncToken<Void> Initialize(IAppSceneManager scenes)
        {
            var token = new AsyncToken<Void>();

            _bootstrapper.BootstrapCoroutine(Delay(
                IMPORT_DELAY_SEC,
                () => token.Succeed(Void.Instance)));

            return token;
        }

        /// <inheritdoc />
        public IAsyncToken<Void> Anchor(string id, GameObject gameObject)
        {
            var token = new AsyncToken<Void>();

            _bootstrapper.BootstrapCoroutine(Delay(
                IMPORT_DELAY_SEC,
                () => token.Succeed(Void.Instance)));

            return token;
        }

        /// <inheritdoc />
        public void UnAnchor(GameObject gameObject)
        {
            //
        }
        
        /// <summary>
        /// Delays a callback.
        /// </summary>
        /// <param name="secs">Seconds to delay.</param>
        /// <param name="callback">The callback to execute.</param>
        /// <returns></returns>
        private IEnumerator Delay(int secs, Action callback)
        {
            yield return new WaitForSecondsRealtime(secs);

            callback();
        }

        /// <summary>
        /// Retrieves a random payload.
        /// </summary>
        private byte[] RandomPayload()
        {
            // random 4 MB payload
            var bytes = new byte[4 * 1000];
            _rand.NextBytes(bytes);

            return bytes;
        }
    }
}