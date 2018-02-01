using System;
using System.Collections;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using UnityEngine;
using UnityEngine.Networking;
using Void = CreateAR.Commons.Unity.Async.Void;

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
        public IAsyncToken<Void> Load(string url, Texture2D texture)
        {
            var token = new AsyncToken<Void>();

            if (null == texture)
            {
                throw new ArgumentException("'texture' cannot be null.");
            }

            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentException("'url' must be non-empty.");
            }

            url = Replace(url);

            byte[] bytes;
            if (_cache.TryGetValue(url, out bytes))
            {
                texture.LoadImage(bytes);

                token.Succeed(Void.Instance);
            }
            else
            {
                Log.Info(this, "Requesting image at {0}.", url);

                _bootstrapper.BootstrapCoroutine(Wait(
                    UnityWebRequest.Get(url),
                    texture,
                    token));
            }

            return token;
        }

        /// <summary>
        /// Waits on a request.
        /// </summary>
        /// <param name="request">The request to wait on.</param>
        /// <param name="texture">The texture to load the image into.</param>
        /// <param name="token">The token to resolve.</param>
        /// <returns></returns>
        private IEnumerator Wait(
            UnityWebRequest request,
            Texture2D texture,
            AsyncToken<Void> token)
        {
            yield return request.SendWebRequest();

            if (request.isHttpError || request.isNetworkError)
            {
                token.Fail(new Exception(request.error));
            }
            else
            {
                if (texture.LoadImage(request.downloadHandler.data))
                {
                    token.Succeed(Void.Instance);
                }
                else
                {
                    token.Fail(new Exception("Could not load data into Texture2D."));
                }
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