using System;
using CreateAR.Commons.Unity.Async;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// <c>IFileSystem</c> implementation that always fails.
    /// </summary>
    public class NullFileSystem : IFileSystem
    {
        /// <inheritdoc />
        public bool Exists(string uri)
        {
            return false;
        }

        /// <inheritdoc cref="IFileSystem"/>
        public IAsyncToken<File<byte[]>> Get(string uri)
        {
            return new AsyncToken<File<byte[]>>(new NotImplementedException());
        }

        /// <inheritdoc cref="IFileSystem"/>
        public IAsyncToken<File<byte[]>> Set(File<byte[]> fileData)
        {
            return new AsyncToken<File<byte[]>>(new NotImplementedException());
        }
    }
}