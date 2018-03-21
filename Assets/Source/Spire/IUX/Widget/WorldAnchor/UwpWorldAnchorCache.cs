#if !UNITY_EDITOR && UNITY_WSA

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.Assets;

using Windows.Storage;

namespace CreateAR.SpirePlayer.IUX
{
    public class UwpWorldAnchorCache : IWorldAnchorCache
    {
        private readonly IHashProvider _hashProvider;
        private readonly string _basePath;

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

        public bool Contains(string id)
        {
            var path = GetDiskPath(id);

            return File.Exists(path);
        }

        public void Save(string id, byte[] bytes)
        {
            var path = GetDiskPath(id);
            if (File.Exists(path))
            {
                return;
            }

            WriteBytes(path, bytes);
        }

        public IAsyncToken<byte[]> Load(string id)
        {
            var path = GetDiskPath(id);
            if (!File.Exists(path))
            {
                return new AsyncToken<byte[]>(new Exception("Does not existing in cache."));
            }

            var token = new AsyncToken<byte[]>();

            Load(path, token);

            return token;
        }

        private static async Task Load(string path, AsyncToken<byte[]> token)
        {
            var file = await StorageFile.GetFileFromPathAsync(path);
            using (var stream = await file.OpenStreamForReadAsync())
            {
                using (var memory = new MemoryStream())
                {
                    await stream.CopyToAsync(memory);

                    token.Succeed(memory.ToArray());
                }
            }
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

        /// <summary>
        /// Writes bytes to disk, asynchronously.
        /// </summary>
        /// <param name="path">The path at which to write.</param>
        /// <param name="bytes">The bytes to write.</param>
        private async void WriteBytes(string path, byte[] bytes)
        {
            using (var file = File.OpenWrite(path))
            {
                try
                {
                    await file.WriteAsync(bytes, 0, bytes.Length);
                }
                catch (Exception exception)
                {
                    Log.Error(this, "Could not write to {0} : {1}.",
                        path,
                        exception);
                }
            }
        }
    }
}

#endif