using System;
using System.Collections;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using UnityEngine;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.EnkluPlayer.IUX
{
    /// <summary>
    /// Passthrough implementation for non-AR platforms. Generates random data
    /// with a real delay.
    /// </summary>
    public class PassthroughAnchorStore : IAnchorStore
    {
        /// <summary>
        /// For debugging.
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
        public PassthroughAnchorStore(IBootstrapper bootstrapper)
        {
            _bootstrapper = bootstrapper;
        }

        /// <inheritdoc />
        public void ClearAllAnchors()
        {
            //
        }

        /// <inheritdoc />
        public IAsyncToken<Void> Export(string id, int version, GameObject gameObject)
        {
            var token = new AsyncToken<Void>();

            _bootstrapper.BootstrapCoroutine(Delay(
                EXPORT_DELAY_SEC,
                () => token.Succeed(Void.Instance)));

            return token;
        }
        
        /// <inheritdoc />
        public IAsyncToken<Void> Setup(
            IElementTxnManager txns,
            IAppSceneManager scenes)
        {
            var token = new AsyncToken<Void>();

            _bootstrapper.BootstrapCoroutine(Delay(
                IMPORT_DELAY_SEC,
                () => token.Succeed(Void.Instance)));

            return token;
        }

        /// <inheritdoc />
        public void Teardown()
        {
            // 
        }

        /// <inheritdoc />
        public void Anchor(string id, int version, GameObject gameObject)
        {
            // do nothing
        }

        /// <inheritdoc />
        public void UnAnchor(GameObject gameObject)
        {
            // do nothing
        }
        
        /// <summary>
        /// Delays a callback.
        /// </summary>
        /// <param name="secs">Seconds to delay.</param>
        /// <param name="callback">The callback to execute.</param>
        /// <returns></returns>
        private static IEnumerator Delay(int secs, Action callback)
        {
            yield return new WaitForSecondsRealtime(secs);

            callback();
        }
    }
}