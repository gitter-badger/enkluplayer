using CreateAR.Commons.Unity.Async;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// FileSystem implementation.
    /// </summary>
    public interface IFileSystem
    {
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
