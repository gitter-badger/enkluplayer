using System;
using System.Diagnostics;
using System.Text;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.Commons.Unity.Storage
{
    /// <summary>
    /// Thin worker class that makes Http requests and does a small bit of
    /// serialization.
    /// </summary>
    public class StorageWorker : IStorageWorker
    {
        /// <summary>
        /// Root endpoint.
        /// </summary>
        private const string ENDPOINT_KVS = "trellis://kv";

        /// <summary>
        /// Dependencies.
        /// </summary>
        private readonly IHttpService _http;
        private readonly ISerializer _serializer;

        /// <inheritdoc cref="IStorageWorker"/>
        public event Action<string> OnDelete;

        /// <summary>
        /// Creates a new StorageWorker.
        /// </summary>
        /// <param name="http">IHttpService implementation.</param>
        /// <param name="serializer">For JSON serialization.</param>
        public StorageWorker(
            IHttpService http,
            ISerializer serializer)
        {
            _http = http;
            _serializer = serializer;
        }

        /// <inheritdoc cref="IStorageWorker"/>
        public IAsyncToken<KvModel[]> GetAll()
        {
            var token = new AsyncToken<KvModel[]>();

            LogVerbose("GetAll()");
            
            _http
                .Get<GetAllKvsResponse>(_http.Urls.Url(ENDPOINT_KVS))
                .OnSuccess(response =>
                {
                    if (null == response.Payload)
                    {
                        token.Fail(new Exception(string.Format(
                            "Unknown error : {0}.",
                            Encoding.UTF8.GetString(response.Raw))));
                        return;
                    }

                    if (!response.Payload.success)
                    {
                        token.Fail(new Exception(response.Payload.error));
                        return;
                    }

                    token.Succeed(response.Payload.body);
                })
                .OnFailure(token.Fail);

            return token;
        }

        /// <inheritdoc cref="IStorageWorker"/>
        public IAsyncToken<KvModel> Create(object value, string tags)
        {
            var token = new AsyncToken<KvModel>();

            LogVerbose("Create()");

            // serialize value
            var serialized = Serialize(value);

            _http
                .Post<CreateKvResponse>(
                    _http.Urls.Url(ENDPOINT_KVS),
                    new CreateKvRequest
                    {
                        value = serialized,
                        tags = tags
                    })
                .OnSuccess(response =>
                {
                    if (null == response.Payload)
                    {
                        token.Fail(new Exception(string.Format(
                            "Unknown error : {0}.",
                            Encoding.UTF8.GetString(response.Raw))));
                        return;
                    }

                    if (!response.Payload.success)
                    {
                        token.Fail(new Exception(response.Payload.error));
                        return;
                    }

                    token.Succeed(response.Payload.body);
                })
                .OnFailure(token.Fail);

            return token;
        }

        /// <inheritdoc cref="IStorageWorker"/>
        public IAsyncToken<object> Load(string key, Type type)
        {
            var token = new AsyncToken<object>();

            LogVerbose("Load({0})", key);

            _http
                .Get<GetKvResponse>(_http.Urls.Url(string.Format(
                    "{0}/{1}",
                    ENDPOINT_KVS,
                    key)))
                .OnSuccess(response =>
                {
                    if (null == response.Payload)
                    {
                        token.Fail(new Exception(string.Format(
                            "Unknown error : {0}.",
                            Encoding.UTF8.GetString(response.Raw))));
                        return;
                    }

                    if (!response.Payload.success)
                    {
                        token.Fail(new Exception(response.Payload.error));
                        return;
                    }

                    var bytes = Encoding.UTF8.GetBytes(response.Payload.body.value);
                    object value;
                    _serializer.Deserialize(type, ref bytes, out value);

                    token.Succeed(value);
                })
                .OnFailure(token.Fail);

            return token;
        }

        /// <inheritdoc cref="IStorageWorker"/>
        public IAsyncToken<Void> Save(string key, object value, string tags, int version)
        {
            var token = new AsyncToken<Void>();

            LogVerbose("Save({0})", key);
            
            _http
                .Put<UpdateKvResponse>(
                    _http.Urls.Url(string.Format(
                        "{0}/{1}",
                        ENDPOINT_KVS,
                        key)),
                    new UpdateKvRequest
                    {
                        value = value,
                        version = version
                    })
                .OnSuccess(response =>
                {
                    if (null == response.Payload)
                    {
                        token.Fail(new Exception(string.Format(
                            "Unknown error : {0}.",
                            Encoding.UTF8.GetString(response.Raw))));
                        return;
                    }

                    if (response.Payload.success)
                    {
                        token.Succeed(Void.Instance);
                    }
                    else
                    {
                        token.Fail(new Exception(response.Payload.error));
                    }
                })
                .OnFailure(token.Fail);

            return token;
        }

        /// <inheritdoc cref="IStorageWorker"/>
        public IAsyncToken<Void> Delete(string key)
        {
            var token = new AsyncToken<Void>();

            LogVerbose("Delete({0})");

            _http
                .Delete<CreateKvResponse>(_http.Urls.Url(string.Format(
                        "{0}/{1}",
                        ENDPOINT_KVS,
                        key)))
                .OnSuccess(response =>
                {
                    if (null == response.Payload)
                    {
                        token.Fail(new Exception(string.Format(
                            "Unknown error : {0}.",
                            Encoding.UTF8.GetString(response.Raw))));
                        return;
                    }

                    if (!response.Payload.success)
                    {
                        token.Fail(new Exception(response.Payload.error));
                        return;
                    }

                    if (null != OnDelete)
                    {
                        OnDelete.Invoke(key);
                    }

                    token.Succeed(Void.Instance);
                })
                .OnFailure(token.Fail);

            return token;
        }

        /// <summary>
        /// Serializes value.
        /// </summary>
        /// <param name="value">The value!</param>
        /// <returns></returns>
        private string Serialize(object value)
        {
            byte[] bytes;
            _serializer.Serialize(value, out bytes);
            var serialized = Encoding.UTF8.GetString(bytes);
            return serialized;
        }

        /// <summary>
        /// Logging.
        /// </summary>
        [Conditional("VERBOSE_LOGGING")]
        private void LogVerbose(string message, params object[] replacements)
        {
            Log.Info(this, message, replacements);
        }
    }
}