using System;
using System.IO;
using System.Text;

using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.Assets;

namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// Uwp implementation of anchor cache.
    /// </summary>
    public class UwpWorldAnchorCache : IWorldAnchorCache
    {
        /// <summary>
        /// Hashes names.
        /// </summary>
        private readonly IHashProvider _hashProvider;

        /// <summary>
        /// Base path to write to.
        /// </summary>
        private readonly string _basePath;

        /// <summary>
        /// Constructor.
        /// </summary>
        public UwpWorldAnchorCache(IHashProvider hashProvider)
        {
            _hashProvider = hashProvider;
            _basePath = Path.Combine(
                UnityEngine.Application.persistentDataPath,
                "HoloAnchors");

            if (!Directory.Exists(_basePath))
            {
                Directory.CreateDirectory(_basePath);
            }
        }

        /// <inheritdoc />
        public bool Contains(string id)
        {
            Log.Info(this, "Contains({0})", id);

            var path = GetDiskPath(id);

            return File.Exists(path);
        }

        /// <inheritdoc />
        public void Save(string id, byte[] bytes)
        {
            var path = GetDiskPath(id);
            if (File.Exists(path))
            {
                return;
            }
            
            File.WriteAllBytes(path, bytes);
        }

        /// <inheritdoc />
        public IAsyncToken<byte[]> Load(string id)
        {
            var path = GetDiskPath(id);
            if (!File.Exists(path))
            {
                return new AsyncToken<byte[]>(new Exception("Does not existing in cache."));
            }

            var token = new AsyncToken<byte[]>();
            
            token.Succeed(File.ReadAllBytes(path));
            
            return token;
        }
        
        /// <summary>
        /// Retrieves a unique, deterministic file path for an id.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        private string GetDiskPath(string id)
        {
            var bytes = Encoding.UTF8.GetBytes(id);
            var hash = _hashProvider.Hash(bytes);
            var encodedHash = Convert
                .ToBase64String(hash)
                // get rid of invalid path characters
                .Replace("+", "")
                .Replace("=", "")
                .Replace("/", "");
            var path = Path.Combine(
                _basePath,
                encodedHash);

            return path;
        }
    }
}