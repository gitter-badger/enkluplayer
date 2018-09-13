using System.IO;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;

namespace CreateAR.EnkluPlayer.IUX
{
    /// <summary>
    /// Uwp implementation of anchor cache.
    /// </summary>
    public class StandardWorldAnchorCache : IWorldAnchorCache
    {
        /// <summary>
        /// File manager.
        /// </summary>
        private readonly IFileManager _files;
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public StandardWorldAnchorCache(IFileManager files)
        {
            _files = files;
            _files.Register("worldanchors://",
                new PassthroughSerializer(),
                new LocalFileSystem(Path.Combine(
                    UnityEngine.Application.persistentDataPath,
                    "WorldAnchors")));
        }

        /// <inheritdoc />
        public bool Contains(string id, int version)
        {
            return _files.Exists(Uri(id, version));
        }

        /// <inheritdoc />
        public void Save(string id, int version, byte[] bytes)
        {
            _files
                .Set(Uri(id, version), bytes)
                .OnFailure(exception => Log.Warning(this, "Could not save world anchor : {0}.", exception));
        }

        /// <inheritdoc />
        public IAsyncToken<byte[]> Load(string id, int version)
        {
            return Async.Map(
                _files.Get<byte[]>(Uri(id, version)),
                file => file.Data);
        }

        /// <summary>
        /// Retrieves a unique, deterministic file path for an id.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="version">Version.</param>
        /// <returns></returns>
        private static string Uri(string id, int version)
        {
            return string.Format(
                "worldanchors://{0}/{1}",
                id,
                version);
        }
    }
}