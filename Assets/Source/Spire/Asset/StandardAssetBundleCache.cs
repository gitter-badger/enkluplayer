using System;
using System.Collections;
using System.IO;
using System.Text;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using UnityEngine;

namespace CreateAR.SpirePlayer.Assets
{
    /// <summary>
    /// Standard disk-cache for bundles.
    /// </summary>
    public class StandardAssetBundleCache : IAssetBundleCache
    {
        /// <summary>
        /// Write state used internally for writing a bundle to disk.
        /// </summary>
        private class WriteState
        {
            /// <summary>
            /// Active stream.
            /// </summary>
            public readonly FileStream Stream;
            
            /// <summary>
            /// Uri of the bundle.
            /// </summary>
            public readonly string Uri;

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="stream">Active stream</param>
            /// <param name="uri">Uri of the bundle.</param>
            public WriteState(FileStream stream, string uri)
            {
                Stream = stream;
                Uri = uri;
            }
        }
        
        /// <summary>
        /// Bootstraps coroutines.
        /// </summary>
        private readonly IBootstrapper _bootstrapper;

        /// <summary>
        /// Hashing function.
        /// </summary>
        private readonly IHashProvider _hashProvider;
        
        /// <summary>
        /// Base path on disk.
        /// </summary>
        private readonly string _basePath;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="bootstrapper">For coroutines.</param>
        /// <param name="hashing">Hashing method.</param>
        /// <param name="basePath">Base path on disk.</param>
        public StandardAssetBundleCache(
            IBootstrapper bootstrapper,
            IHashProvider hashing,
            string basePath)
        {
            _bootstrapper = bootstrapper;
            _hashProvider = hashing;
            _basePath = basePath;
        }
        
        /// <inheritdoc cref="IAssetBundleCache"/>
        public bool Contains(string uri)
        {
            return File.Exists(FilePath(uri));
        }

        /// <inheritdoc cref="IAssetBundleCache"/>
        public IAsyncToken<AssetBundle> Load(string uri, out LoadProgress progress)
        {
            var token = new AsyncToken<AssetBundle>();

            progress = new LoadProgress();
            
            if (!Contains(uri))
            {
                token.Fail(new Exception("Could not find bundle in cache."));

                progress.Value = 1;
            }
            else
            {
                var path = FilePath(uri);
                var request = AssetBundle.LoadFromFileAsync(path);
                
                _bootstrapper.BootstrapCoroutine(WaitOnLoad(
                    token,
                    request,
                    progress));
            }
            
            return token;
        }
        
        /// <inheritdoc cref="IAssetBundleCache"/>
        public void Save(string uri, byte[] bytes)
        {
            var path = FilePath(uri);
            if (File.Exists(path))
            {
                return;
            }

            var dir = Path.GetDirectoryName(path);
            if (string.IsNullOrEmpty(dir))
            {
                Log.Warning(this, "Invalid path has no directory : {0}.", path);
                return;
            }

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            
            Log.Info(this, "Writing bundle to disk. [{0}, {1}].", uri, path);

            try
            {
                using (var stream = File.OpenWrite(path))
                {
#if NETFX_CORE
                    stream.WriteAsync(bytes, 0, bytes.Length);

                    Log.Info(this,
                        "Bundle successfully written to disk for {0}.",
                        uri);
#else
                    stream.BeginWrite(
                        bytes,
                        0,
                        bytes.Length,
                        EndWrite,
                        new WriteState(stream, uri));
#endif
                }
            }
            catch (Exception exception)
            {
                Log.Error(this, "Could not write bundle to disk : [{0}, {1}].",
                    uri,
                    exception);
            }
        }

        /// <summary>
        /// Waits for the asset bundle load to complete.
        /// </summary>
        /// <param name="token">The token to resolve.</param>
        /// <param name="request">The unity request.</param>
        /// <param name="progress">Progress indicator.</param>
        /// <returns></returns>
        private IEnumerator WaitOnLoad(
            AsyncToken<AssetBundle> token,
            AssetBundleCreateRequest request,
            LoadProgress progress)
        {
            while (!request.isDone)
            {
                progress.Value = request.progress;

                yield return null;
            }

            progress.Value = 1;

            if (null == request.assetBundle)
            {
                token.Fail(new Exception("There was an error loading the bundle."));
            }
            else
            {
                token.Succeed(request.assetBundle);
            }
        }
        
        /// <summary>
        /// Called from a thread pool when write has completed.
        /// </summary>
        /// <param name="result">Result.</param>
        private void EndWrite(IAsyncResult result)
        {
#if !NETFX_CORE
            var state = (WriteState) result.AsyncState;
            
            var stream = state.Stream;
            stream.EndWrite(result);
            stream.Dispose();
            
            Log.Info(this,
                "Bundle successfully written to disk for {0}.",
                state.Uri);
#endif
        }
        
        /// <summary>
        /// Retrieves a unique, deterministic file path for a uri.
        /// </summary>
        /// <param name="uri">The Uri.</param>
        /// <returns></returns>
        private string FilePath(string uri)
        {
            var bytes = Encoding.UTF8.GetBytes(uri);
            var hash = _hashProvider.Hash(bytes);
            var encodedHash = Convert.ToBase64String(hash);
            var path = Path.Combine(
                _basePath,
                encodedHash);
            
            Log.Info(this, "Hash({0}) = {1}",
                uri,
                path);

            return path;
        }
    }
}