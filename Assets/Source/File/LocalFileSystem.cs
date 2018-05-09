using System;
using System.Diagnostics;
using System.IO;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// <c>IFileSystem</c> implementation that reads from and writes to the
    /// local disk.
    /// </summary>
    public class LocalFileSystem : IFileSystem
    {
        /// <summary>
        /// Base path to append.
        /// </summary>
        private readonly string _basePath;

        /// <summary>
        /// Creates a new LocalFileSystem.
        /// </summary>
        /// <param name="basePath">The base path.</param>
        public LocalFileSystem(string basePath)
        {
            _basePath = basePath;
        }

        /// <inheritdoc cref="IFileSystem"/>
        public bool Exists(string uri)
        {
            return File.Exists(Path.Combine(
                _basePath,
                RelativeUri(uri) + ".local"));
        }

        /// <inheritdoc cref="IFileSystem"/>
        public IAsyncToken<File<byte[]>> Get(string uri)
        {
            var relUri = RelativeUri(uri);
            var path = Path.Combine(
                _basePath,
                relUri) + ".local";

            Verbose("Get({0})", path);
            
            byte[] bytes;
            try
            {
                bytes = File.ReadAllBytes(path);
            }
            catch (Exception exception)
            {
                return new AsyncToken<File<byte[]>>(exception);
            }
            
            return new AsyncToken<File<byte[]>>(new File<byte[]>(
                uri,
                bytes));
        }

        /// <inheritdoc cref="IFileSystem"/>
        public IAsyncToken<File<byte[]>> Set(File<byte[]> file)
        {
            var relUri = RelativeUri(file.Uri);
            var path = Path.Combine(
                _basePath,
                relUri) + ".local";

            Verbose("Set({0})", path);

            var directory = Path.GetDirectoryName(path) ?? ".";

            try
            {
                Directory.CreateDirectory(directory);
            }
            catch (Exception exception)
            {
                return new AsyncToken<File<byte[]>>(exception);
            }

            try
            {
                File.WriteAllBytes(path, file.Data);
            }
            catch (Exception exception)
            {
                return new AsyncToken<File<byte[]>>(exception);
            }

            return new AsyncToken<File<byte[]>>(file);
        }

        /// <summary>
        /// Trims the protocol off a Uri.
        /// </summary>
        /// <param name="uri">The uri to trim.</param>
        /// <returns>A trimmed Uri.</returns>
        private string RelativeUri(string uri)
        {
            return uri.Substring(uri.IndexOf("://", StringComparison.Ordinal) + 3);
        }

        /// <summary>
        /// Verbose logging.
        /// </summary>
        [Conditional("LOGGING_VERBOSE")] 
        private void Verbose(string message, params object[] replacements)
        {
            Log.Info(this, message, replacements);
        }
    }
}