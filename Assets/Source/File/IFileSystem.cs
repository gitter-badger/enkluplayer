using CreateAR.Commons.Unity.Async;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// FileSystem implementation.
    /// </summary>
    public interface IFileSystem
    {
        /// <summary>
        /// True iff a file exists at a uri.
        /// </summary>
        /// <param name="uri">the uri.</param>
        /// <returns></returns>
        bool Exists(string uri);

        /// <summary>
        /// Retrieves a file.
        /// </summary>
        /// <param name="uri">Uri of the file.</param>
        /// <returns></returns>
        IAsyncToken<File<byte[]>> Get(string uri);

        /// <summary>
        /// Sets a file.
        /// </summary>
        /// <param name="file">The file that was set.</param>
        /// <returns></returns>
        IAsyncToken<File<byte[]>> Set(File<byte[]> file);
    }
}
