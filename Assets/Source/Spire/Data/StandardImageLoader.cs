using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
        /// <summary>
        /// Custom handling for Resources.
        /// </summary>
        private const string RESOURCE_PROTOCOL = "res://";

        /// <summary>
        /// Dumb in-memory cache.
        /// </summary>
        private readonly Dictionary<string, byte[]> _cache = new Dictionary<string, byte[]>();
        
        /// <summary>
        /// Bootstraps coroutines.
        /// </summary>
        private readonly IBootstrapper _bootstrapper;

        /// <inheritdoc />
        public UrlFormatterCollection Urls { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public StandardImageLoader(
            IBootstrapper bootstrapper,
            UrlFormatterCollection urls)
        {
            _bootstrapper = bootstrapper;

            Urls = urls;
        }
        
        /// <inheritdoc />
        public IAsyncToken<ManagedTexture> Load(string url)
        {
            var token = new AsyncToken<ManagedTexture>();
            
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentException("'url' must be non-empty.");
            }
            
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
                if (url.StartsWith(RESOURCE_PROTOCOL))
                {
                    var path = url.Substring(RESOURCE_PROTOCOL.Length);

                    LogVerbose("Requesting image from resources at {0}.", url);

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

                url = Urls.Url(url);

                try
                {
                    // ReSharper disable once UnusedVariable
                    var test = new Uri(url);
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
        /// Retrieves a texture.
        /// 
        /// TODO: Pool.
        /// </summary>
        /// <returns></returns>
        private ManagedTexture ManagedTexture(Texture2D texture = null)
        {
            return new ManagedTexture(texture ?? new Texture2D(2, 2));
        }

        /// <summary>
        /// Verbose logging!
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="replacements">Replacements.</param>
        //[Conditional("LOGGING_VERBOSE")]
        private void LogVerbose(string message, params object[] replacements)
        {
            Log.Info(this, message, replacements);
        }
    }
}