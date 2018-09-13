using System;
using System.IO;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Standard implementation of a script cache.
    /// </summary>
    public class StandardScriptCache : IScriptCache
    {
        /// <summary>
        /// Manages files.
        /// </summary>
        private readonly IFileManager _files;

        /// <summary>
        /// Constructor.
        /// </summary>
        public StandardScriptCache(IFileManager files)
        {
            _files = files;
            _files.Register(
                "scripts://",
                new UTF8Serializer(),
                new LocalFileSystem(Path.Combine(
                    UnityEngine.Application.persistentDataPath,
                    "Scripts")));
        }
        
        /// <inheritdoc />
        public bool Contains(string id, int version)
        {
            return _files.Exists(Uri(id, version));
        }

        /// <inheritdoc />
        public void Save(string id, int version, string value)
        {
            _files
                .Set(Uri(id, version), value)
                .OnFailure(exception => Log.Warning(
                    this,
                    "Could not save script to disk : {0}.",
                    exception));
        }

        /// <inheritdoc />
        public IAsyncToken<string> Load(string id, int version)
        {
            return Async.Map(
                _files.Get<string>(Uri(id, version)),
                file => file.Data);
        }

        /// <inheritdoc />
        public void Purge(DateTime cutoff)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns a URI for a script id.
        /// </summary>
        /// <param name="id">The id of the script.</param>
        /// <param name="version">The version of the script.</param>
        /// <returns></returns>
        private static string Uri(string id, int version)
        {
            return string.Format(
                "scripts://{0}/v{1}",
                id,
                version);
        }
    }
}