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
        /// Protocol for resources.
        /// </summary>
        private const string RESOURCE_PROTOCOL = "res://";

        /// <summary>
        /// Dumb in-memory cache.
        /// </summary>
        private readonly Dictionary<string, byte[]> _cache = new Dictionary<string, byte[]>();

        /// <summary>
        /// List of replacements.
        /// </summary>
        private readonly List<ReplacementRecord> _replacements = new List<ReplacementRecord>();

        /// <summary>
        /// List of protocol replacements.
        /// </summary>
        private readonly List<ReplacementRecord> _protocols = new List<ReplacementRecord>();

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
        public IImageLoader ReplaceProtocol(string protocol, string replacement)
        {
            if (!protocol.EndsWith("://"))
            {
                protocol += "://";
            }

            _protocols.Add(new ReplacementRecord(protocol, replacement));

            return this;
        }

        /// <inheritdoc />
        public IAsyncToken<ManagedTexture> Load(string url)
        {
            var token = new AsyncToken<ManagedTexture>();
            
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentException("'url' must be non-empty.");
            }

            url = Replace(url);

            byte[] bytes;
            if (_cache.TryGetValue(url, out bytes))
            {
                var texture = ManagedTexture();
                if (texture.Source.LoadImage(bytes))
                {
                    token.Succeed(texture);
                }
                else
                {
                    token.Fail(new Exception("Could not load bytes into texture."));
                }
            }
            else
            {
                Log.Info(this, "Requesting image at {0}.", url);

                if (url.StartsWith(RESOURCE_PROTOCOL))
                {
                    var path = url.Substring(RESOURCE_PROTOCOL.Length);
                    var source = Resources.Load<Texture2D>(path);
                    if (null == source)
                    {
                        token.Fail(new Exception(string.Format(
                            "Could not find {0} in Resources.",
                            path)));
                    }
                    else
                    {
                        var texture = ManagedTexture(source);
                        token.Succeed(texture);
                    }

                    return token;
                }

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
            AsyncToken<ManagedTexture> token)
        {
            yield return request.SendWebRequest();

            if (request.isHttpError || request.isNetworkError)
            {
                token.Fail(new Exception(request.error));
            }
            else
            {
                var texture = ManagedTexture();
                var bytes = request.downloadHandler.data;
                if (texture.Source.LoadImage(bytes))
                {
                    _cache[request.url] = bytes;

                    token.Succeed(texture);
                }
                else
                {
                    token.Fail(new Exception("Could not load bytes into texture."));
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
            var index = url.IndexOf("://", StringComparison.Ordinal);
            if (-1 != index)
            {
                var protocol = url.Substring(0, index + 3);
                for (int i = 0, len = _protocols.Count; i < len; i++)
                {
                    if (_protocols[i].Template == protocol)
                    {
                        url = _protocols[i].Replacement + url.Substring(index + 3);

                        break;
                    }
                }
            }

            for (var i = 0; i < _replacements.Count; i++)
            {
                var replacement = _replacements[i];

                url = url.Replace(
                    string.Format(@"{{{0}}}", replacement.Template),
                    replacement.Replacement);
            }

            return url;
        }

        /// <summary>
        /// Retrieves a texture.
        /// 
        /// TODO: Pool.
        /// </summary>
        /// <returns></returns>
        private ManagedTexture ManagedTexture(Texture2D texture = null)
        {
            return new ManagedTexture(texture ?? new Texture2D(2, 2));
        }
    }
}