using System;
using System.IO;
using CreateAR.Commons.Unity.Async;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Standard implementation of anchor cache.
    /// </summary>
    public class StandardDiskCache : IDiskCache
    {
        /// <summary>
        /// Base path to write to.
        /// </summary>
        private readonly string _basePath;

        /// <summary>
        /// Constructor.
        /// </summary>
        public StandardDiskCache(string basePath)
        {
            _basePath = Path.Combine(
                UnityEngine.Application.persistentDataPath,
                basePath);

            if (!Directory.Exists(_basePath))
            {
                Directory.CreateDirectory(_basePath);
            }
        }

        /// <inheritdoc />
        public bool Contains(string id)
        {
            return File.Exists(GetDiskPath(id));
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

        /// <inheritdoc />
        public void Purge(DateTime cutoff)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Retrieves a unique, deterministic file path for an id.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        private string GetDiskPath(string id)
        {
            return Path.Combine(
                _basePath,
                Path.Combine(id, ".file"));
        }
    }
}