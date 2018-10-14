using System.Collections;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using UnityEngine;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Adds artificial delay to script cache.
    /// </summary>
    public class DelayedScriptCache : StandardScriptCache
    {
        /// <summary>
        /// Bootstraps coroutines.
        /// </summary>
        private readonly IBootstrapper _bootstrapper;

        /// <summary>
        /// Constructor.
        /// </summary>
        public DelayedScriptCache(
            IFileManager files,
            IBootstrapper bootstrapper) : base(files)
        {
            _bootstrapper = bootstrapper;
        }

        /// <inheritdoc />
        public override IAsyncToken<string> Load(string id, int version)
        {
            var internalToken = base.Load(id, version);
            var externalToken = new AsyncToken<string>();

            _bootstrapper.BootstrapCoroutine(Wait(
                internalToken,
                externalToken));

            return externalToken;
        }

        /// <summary>
        /// Waits before adding callbacks to internal token.
        /// </summary>
        /// <param name="internalToken">Token used internally.</param>
        /// <param name="externalToken">Token used externally.</param>
        /// <returns></returns>
        private IEnumerator Wait(
            IAsyncToken<string> internalToken,
            AsyncToken<string> externalToken)
        {
            yield return new WaitForSeconds(1f);

            internalToken
                .OnSuccess(externalToken.Succeed)
                .OnFailure(externalToken.Fail);
        }
    }
}