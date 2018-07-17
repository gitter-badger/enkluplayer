using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;

namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// Standard implementation of <c>IScanLoader</c> that caches scans.
    /// </summary>
    public class StandardScanLoader : IScanLoader
    {
        /// <summary>
        /// Caches HTTP requests.
        /// </summary>
        private readonly HttpRequestCacher _cache;

        /// <summary>
        /// Formats urls.
        /// </summary>
        private readonly UrlFormatterCollection _urls;

        /// <summary>
        /// Makes HTTP requests.
        /// </summary>
        private readonly IHttpService _http;

        /// <summary>
        /// Constructor.
        /// </summary>
        public StandardScanLoader(
            HttpRequestCacher cache,
            UrlFormatterCollection urls,
            IHttpService http)
        {
            _cache = cache;
            _urls = urls;
            _http = http;
        }

        /// <inheritdoc />
        public IAsyncToken<byte[]> Load(string uri)
        {
            return _cache.Request(
                HttpRequestCacher.LoadBehavior.NetworkFirst,
                uri,
                () => _http.Download(_urls.Url(uri)));
        }
    }
}