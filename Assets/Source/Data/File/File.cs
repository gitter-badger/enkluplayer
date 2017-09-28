namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Represents a file. Immutable.
    /// </summary>
    public class File<T>
    {
        /// <summary>
        /// Uri, including the protocol.
        /// </summary>
        public readonly string Uri;

        /// <summary>
        /// File contents.
        /// </summary>
        public readonly T Data;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="uri">Uri, including protocol.</param>
        /// <param name="data">Data.</param>
        public File(string uri, T data)
        {
            Uri = uri;
            Data = data;
        }
    }
}