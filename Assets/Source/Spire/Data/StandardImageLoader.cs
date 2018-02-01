using System;
using System.Collections;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using UnityEngine;
using UnityEngine.Networking;

namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// Standard implementation of <c>IImageLoader</c>.
    /// 
    /// // TODO: queue, memory cache, disk cache, limit concurrents, etc.
    /// </summary>
    public class StandardImageLoader : IImageLoader
    {
        private class ReplacementRecord
        {
            public readonly string Template;
            public readonly string Replacement;

            public ReplacementRecord(string template, string replacement)
            {
                Template = template;
                Replacement = replacement;
            }
        }

        /// <summary>
        /// Dumb in-memory cache.
        /// </summary>
        private readonly Dictionary<string, byte[]> _cache = new Dictionary<string, byte[]>();

        /// <summary>
        /// List of replacements.
        /// </summary>
        private readonly List<ReplacementRecord> _replacements = new List<ReplacementRecord>();

        /// <summary>
        /// Bootstraps coroutines.
        /// </summary>
        private readonly IBootstrapper _bootstrapper;
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public StandardImageLoader(IBootstrapper bootstrapper)
        {
            _bootstrapper = bootstrapper;
        }

        /// <inheritdoc />
        public IImageLoader Replace(string template, string replacement)
        {
            _replacements.Add(new ReplacementRecord(template, replacement));

            return this;
        }

        /// <inheritdoc />
        public IAsyncToken<Func<Texture2D, bool>> Load(string url)
        {
            var token = new AsyncToken<Func<Texture2D, bool>>();
            
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentException("'url' must be non-empty.");
            }

            url = Replace(url);

            byte[] bytes;
            if (_cache.TryGetValue(url, out bytes))
            {
                token.Succeed(texture =>
                {
                    texture.LoadImage(bytes);

                    return true;
                });
            }
            else
            {
                Log.Info(this, "Requesting image at {0}.", url);

                try
                {
                    new Uri(url);
                }
                catch (Exception exception)
                {
                    token.Fail(exception);

                    return token;
                }

                _bootstrapper.BootstrapCoroutine(Wait(
                    UnityWebRequest.Get(url),
                    token));
            }

            return token;
        }

        /// <summary>
        /// Waits on a request.
        /// </summary>
        /// <param name="request">The request to wait on.</param>
        /// <param name="token">The token to resolve.</param>
        /// <returns></returns>
        private IEnumerator Wait(
            UnityWebRequest request,
            AsyncToken<Func<Texture2D, bool>> token)
        {
            yield return request.SendWebRequest();

            if (request.isHttpError || request.isNetworkError)
            {
                token.Fail(new Exception(request.error));
            }
            else
            {
                token.Succeed(texture =>
                {
                    var bytes = request.downloadHandler.data;
                    if (texture.LoadImage(bytes))
                    {
                        _cache[request.url] = bytes;

                        return true;
                    }

                    return false;
                });
            }
        }

        /// <summary>
        /// Replaces replacements in url.
        /// </summary>
        /// <param name="url">Url to replace.</param>
        /// <returns></returns>
        private string Replace(string url)
        {
            for (var i = 0; i < _replacements.Count; i++)
            {
                var replacement = _replacements[i];

                url = url.Replace(
                    string.Format(@"{{{0}}}", replacement.Template),
                    replacement.Replacement);
            }

            return url;
        }
    }
}