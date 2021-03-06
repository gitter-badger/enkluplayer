﻿using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// IHttpService implementation that always fails.
    /// </summary>
    public class OfflineHttpService : IHttpService
    {
        /// <inheritdoc />
        public UrlFormatterCollection Urls { get; private set; }

        /// <inheritdoc />
        public Dictionary<string, string> Headers { get; private set; }

        /// <inheritdoc />
        public long TimeoutMs { get; set; }

        /// <inheritdoc />
        public event Action<string, string, Dictionary<string, string>, object> OnRequest;

        /// <summary>
        /// Constructor.
        /// </summary>
        public OfflineHttpService(UrlFormatterCollection urls)
        {
            Urls = urls;
            Headers = new Dictionary<string, string>();
        }

        /// <inheritdoc />
        public void Abort()
        {
            
        }

        /// <inheritdoc />
        public IAsyncToken<HttpResponse<T>> Get<T>(string url)
        {
            if (null != OnRequest)
            {
                OnRequest("GET", url, Headers, null);
            }

            return new AsyncToken<HttpResponse<T>>(new Exception("Network error."));
        }

        /// <inheritdoc />
        public IAsyncToken<HttpResponse<T>> Post<T>(string url, object payload)
        {
            if (null != OnRequest)
            {
                OnRequest("POST", url, Headers, payload);
            }

            return new AsyncToken<HttpResponse<T>>(new Exception("Network error."));
        }

        /// <inheritdoc />
        public IAsyncToken<HttpResponse<T>> Put<T>(string url, object payload)
        {
            if (null != OnRequest)
            {
                OnRequest("PUT", url, Headers, payload);
            }

            return new AsyncToken<HttpResponse<T>>(new Exception("Network error."));
        }

        /// <inheritdoc />
        public IAsyncToken<HttpResponse<T>> Delete<T>(string url)
        {
            if (null != OnRequest)
            {
                OnRequest("DELETE", url, Headers, null);
            }

            return new AsyncToken<HttpResponse<T>>(new Exception("Network error."));
        }

        /// <inheritdoc />
        public IAsyncToken<HttpResponse<T>> PostFile<T>(string url, IEnumerable<CreateAR.Commons.Unity.DataStructures.Tuple<string, string>> fields, ref byte[] file)
        {
            if (null != OnRequest)
            {
                OnRequest("POST", url, Headers, null);
            }

            return new AsyncToken<HttpResponse<T>>(new Exception("Network error."));
        }

        /// <inheritdoc />
        public IAsyncToken<HttpResponse<T>> PutFile<T>(string url, IEnumerable<CreateAR.Commons.Unity.DataStructures.Tuple<string, string>> fields, ref byte[] file)
        {
            if (null != OnRequest)
            {
                OnRequest("PUT", url, Headers, null);
            }

            return new AsyncToken<HttpResponse<T>>(new Exception("Network error."));
        }

        /// <inheritdoc />
        public IAsyncToken<HttpResponse<byte[]>> Download(string url)
        {
            if (null != OnRequest)
            {
                OnRequest("GET", url, Headers, null);
            }

            return new AsyncToken<HttpResponse<byte[]>>(new Exception("Network error."));
        }
    }
}