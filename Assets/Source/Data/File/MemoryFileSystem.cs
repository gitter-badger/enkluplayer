using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Async;

namespace CreateAR.SpirePlayer.Test
{
    /// <summary>
    /// An in-memory implementation of <c>IFileSystem</c>.
    /// </summary>
    public class MemoryFileSystem : IFileSystem
    {
        /// <summary>
        /// File lookup.
        /// </summary>
        private readonly Dictionary<string, File<byte[]>> _files = new Dictionary<string, File<byte[]>>();

        /// <inheritdoc cref="IFileSystem"/>
        public IAsyncToken<File<byte[]>> Get(string uri)
        {
            File<byte[]> file;
            if (_files.TryGetValue(uri, out file))
            {
                return new AsyncToken<File<byte[]>>(file);
            }

            return new AsyncToken<File<byte[]>>(new Exception(
                "Could not fine file."));
        }

        /// <inheritdoc cref="IFileSystem"/>
        public IAsyncToken<File<byte[]>> Set(File<byte[]> file)
        {
            var setFile = _files[file.Uri] = new File<byte[]>(
                file.Uri,
                file.Data);

            return new AsyncToken<File<byte[]>>(setFile);
        }
    }
}