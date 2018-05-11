using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Describes an object that can get and set files.
    /// </summary>
    public interface IFileManager
    {
        /// <summary>
        /// Configures an <c>IFileSystem</c>.
        /// </summary>
        /// <param name="protocol">The protocol this <c>IFileSystem</c> will handle.</param>
        /// <param name="serializer">An object for serializing/deserializing.</param>
        /// <param name="fileSystem">The <c>IFileSystem</c> to use for all operations.</param>
        void Register(
            string protocol,
            ISerializer serializer,
            IFileSystem fileSystem);

        /// <summary>
        /// Unregisters <c>IFileSystem</c> implementation for a protocol.
        /// </summary>
        /// <param name="protocol">The protocol.</param>
        bool Unregister(string protocol);

        /// <summary>
        /// True iff a file exists at a Uri.
        /// </summary>
        /// <param name="uri">The uri.</param>
        /// <returns></returns>
        bool Exists(string uri);

        /// <summary>
        /// Retrieves a file.
        /// </summary>
        /// <param name="uri">Uri of the File to retrieve.</param>
        /// <returns></returns>
        IAsyncToken<File<T>> Get<T>(string uri);

        /// <summary>
        /// Writes a file.
        /// </summary>
        /// <param name="uri">The Uri to write to.</param>
        /// <param name="value">The value to write.</param>
        /// <returns></returns>
        IAsyncToken<File<T>> Set<T>(string uri, T value);

        /// <summary>
        /// Delete a file at a URI.
        /// </summary>
        /// <param name="uri">The uri in question.</param>
        /// <returns></returns>
        IAsyncToken<Void> Delete(string uri);
    }
}