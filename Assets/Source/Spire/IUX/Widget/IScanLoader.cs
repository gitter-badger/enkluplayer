using CreateAR.Commons.Unity.Async;

namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// Describes an object that can load scans.
    /// </summary>
    public interface IScanLoader
    {
        /// <summary>
        /// Starts a load of scan data.
        /// </summary>
        /// <param name="uri">The URI for the scan.</param>
        /// <returns></returns>
        IAsyncToken<byte[]> Load(string uri);
    }
}