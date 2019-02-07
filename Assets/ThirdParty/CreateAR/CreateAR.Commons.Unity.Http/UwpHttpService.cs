using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.DataStructures;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace CreateAR.Commons.Unity.Http
{
    /// <summary>
    /// Uwp implementation of <see cref="IHttpService"/>
    /// </summary>
    public class UwpHttpService : IHttpService
    {
        protected enum SerializationType
        {
            Json,
            Raw
        }

        /// <summary>
        /// 
        /// </summary>
        protected class OutboundRequest
        {
            public Task<HttpResponseMessage> ResponseTask { get; set; }
            public CancellationTokenSource CancellationToken { get; set; }
            public HttpRequestMessage Request { get; set; }
        }

        /// <summary>
        /// Specifies content types.
        /// </summary>
        private const string CONTENT_TYPE_JSON = "application/json";

        /// <summary>
        /// Serializer!
        /// </summary>
        private readonly ISerializer _serializer;

        /// <summary>
        /// Bootstrapper implementation.
        /// </summary>
        private readonly IBootstrapper _bootstrapper;

        /// <summary>
        /// Http Client to use
        /// </summary>
        private readonly HttpClient _client;

        /// <summary>
        /// Requests.
        /// </summary>
        private readonly List<OutboundRequest> _requestsOut = new List<OutboundRequest>();

        /// <inheritdoc cref="IHttpService"/>
        public UrlFormatterCollection Urls { get; }

        /// <inheritdoc cref="IHttpService"/>
        public Dictionary<string, string> Headers { get; }

        /// <summary>
        /// Creates an UwpHttpService.
        /// 
        /// TODO: This class enforces a json contenttype, but accepts any
        /// TODO: ISerializer.
        /// </summary>
        public UwpHttpService(
            ISerializer serializer,
            IBootstrapper bootstrapper,
            UrlFormatterCollection urls)
        {
            _serializer = serializer;
            _bootstrapper = bootstrapper;
            _client = new HttpClient();

            System.Net.ServicePointManager.ServerCertificateValidationCallback += (o, certificate, chain, errors) =>
            {
                // Gotta figure this out -- this bypasses SSL Errors
                return true;
            };

            Urls = urls;
            Headers = new Dictionary<string, string>();
        }

        /// <inheritdoc cref="IHttpService"/>
        public void Abort()
        {
            foreach (var request in _requestsOut)
            {
                request.CancellationToken.Cancel();
            }

            _requestsOut.Clear();
        }

        /// <inheritdoc cref="IHttpService"/>
        public IAsyncToken<HttpResponse<T>> Get<T>(string url)
        {
            return SendJsonRequest<T>(HttpVerb.Get, url, null);
        }

        /// <inheritdoc cref="IHttpService"/>
        public IAsyncToken<HttpResponse<T>> Post<T>(
            string url,
            object payload)
        {
            return SendJsonRequest<T>(HttpVerb.Post, url, payload);
        }

        /// <inheritdoc cref="IHttpService"/>
        public IAsyncToken<HttpResponse<T>> Put<T>(string url, object payload)
        {
            return SendJsonRequest<T>(HttpVerb.Put, url, payload);
        }

        /// <inheritdoc cref="IHttpService"/>
        public IAsyncToken<HttpResponse<T>> Delete<T>(string url)
        {
            return SendJsonRequest<T>(HttpVerb.Delete, url, null);
        }

        /// <inheritdoc cref="IHttpService"/>
        public IAsyncToken<HttpResponse<T>> PostFile<T>(
            string url,
            IEnumerable<DataStructures.Tuple<string, string>> fields,
            ref byte[] file)
        {
            return SendFile<T>(HttpVerb.Post, url, fields, ref file);
        }

        /// <inheritdoc cref="IHttpService"/>
        public IAsyncToken<HttpResponse<T>> PutFile<T>(
            string url,
            IEnumerable<DataStructures.Tuple<string, string>> fields,
            ref byte[] file)
        {
            return SendFile<T>(HttpVerb.Put, url, fields, ref file);
        }

        /// <inheritdoc cref="IHttpService"/>
        public IAsyncToken<HttpResponse<byte[]>> Download(string url)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);

            ApplyHeaders(Headers, request);

            return Send<byte[]>(request, SerializationType.Raw);
        }

        /// <summary>
        /// Sends a json request.
        /// </summary>
        /// <typeparam name="T">The type of response we expect.</typeparam>
        /// <param name="verb">The http verb to use.</param>
        /// <param name="url">The url to send the request to.</param>
        /// <param name="payload">The object that will be serialized into json.</param>
        /// <returns>An IAsyncToken to listen to.</returns>
        /// <exception cref="NullReferenceException"></exception>
        protected IAsyncToken<HttpResponse<T>> SendJsonRequest<T>(
            HttpVerb verb,
            string url,
            object payload)
        {
            var request = new HttpRequestMessage(MethodFor(verb), url);
            
            ApplyHeaders(Headers, request);
            ApplyJsonPayload(payload, request);

            return Send<T>(request);
        }

        /// <summary>
        /// Sends a file!
        /// </summary>
        /// <typeparam name="T">The type of response we expect.</typeparam>
        /// <param name="verb">The http verb to use.</param>
        /// <param name="url">The url to send the request to.</param>
        /// <param name="fields">Optional fields that will _precede_ the file.</param>
        /// <param name="file">The file, which will be named "file".</param>
        /// <returns></returns>
        private IAsyncToken<HttpResponse<T>> SendFile<T>(
            HttpVerb verb,
            string url,
            IEnumerable<DataStructures.Tuple<string, string>> fields,
            ref byte[] file)
        {
            var request = new HttpRequestMessage(MethodFor(verb), url);
            var form = new MultipartFormDataContent();

            foreach (var tuple in fields)
            {
                form.Add(new StringContent(tuple.Item2), tuple.Item1);
            }

            form.Add(new ByteArrayContent(file), "file");
            request.Content = form;

            ApplyHeaders(Headers, request);

            return Send<T>(request);
        }

        /// <summary>
        /// Applies headers to request.
        /// </summary>
        /// <param name="headers">Headers to add to request.</param>
        /// <param name="request">Request to add headers to.</param>
        protected static void ApplyHeaders(
            Dictionary<string, string> headers,
            HttpRequestMessage request)
        {
            if (headers == null)
            {
                return;
            }

            foreach (var entry in headers)
            {
                request.Headers.TryAddWithoutValidation(entry.Key, entry.Value);
            }
        }

        /// <summary>
        /// Applies json payload to request.
        /// </summary>
        /// <param name="payload">The payload to add to the request.</param>
        /// <param name="request">The request.</param>
        protected void ApplyJsonPayload(object payload, HttpRequestMessage request)
        {
            if (payload == null)
            {
                return;
            }

            byte[] payloadBytes;

            // let serialization errors bubble up
            _serializer.Serialize(payload, out payloadBytes);
            request.Content = new ByteArrayContent(payloadBytes);
            request.Headers.TryAddWithoutValidation("Content-Type", CONTENT_TYPE_JSON);
            request.Headers.TryAddWithoutValidation("Accept", CONTENT_TYPE_JSON);
        }
        
        /// <summary>
        /// Sends an asynchronous http request and return an <see cref="AsyncToken{T}"/> for handling the response
        /// </summary>
        private AsyncToken<HttpResponse<T>> Send<T>(
            HttpRequestMessage request, 
            SerializationType serialization = SerializationType.Json)
        {
            var token = new AsyncToken<HttpResponse<T>>();
            var cancelToken = new CancellationTokenSource();
            var task = _client.SendAsync(request, cancelToken.Token);

            var outRequest = new OutboundRequest
            {
                CancellationToken = cancelToken,
                Request = request,
                ResponseTask = task
            };

            _requestsOut.Add(outRequest);
            
            _bootstrapper.BootstrapCoroutine(WaitFor(
                task,
                // Handle a Completed Send() - Still need to check response for status codes
                response =>
                {
                    var httpResponse = ToHttpResponse<T>(request, response, serialization);
                    _requestsOut.Remove(outRequest);

                    // Apply Success or Failure Conditions Based on Status Code
                    if (httpResponse.NetworkSuccess)
                    {
                        token.Succeed(httpResponse);
                    }
                    else
                    {
                        token.Fail(new Exception(httpResponse.NetworkError));
                    }
                },
                // Error occurred (or cancelled) during the Send
                e =>
                {
                    token.Fail(e);
                }));

            return token;
        }

        /// <summary>
        /// Coroutine for waiting on a <see cref="Task"/>
        /// </summary>
        private IEnumerator WaitFor<T>(Task<T> responseTask, Action<T> onComplete, Action<Exception> onError)
        {
            while (!(responseTask.IsCanceled || responseTask.IsCompleted || responseTask.IsFaulted))
            {
                yield return null;
            }

            if (responseTask.IsCompleted)
            {
                onComplete(responseTask.Result);
            }
            else
            {
                onError(responseTask.Exception);
            }
        }

        /// <summary>
        /// Converts the <see cref="HttpResponseMessage"/> to a <see cref="HttpResponse{T}"/>.
        /// </summary>
        protected HttpResponse<T> ToHttpResponse<T>(
            HttpRequestMessage httpRequest,
            HttpResponseMessage httpResponse,
            SerializationType serialization)
        {
            var response = new HttpResponse<T>
            {
                Headers = FormatHeaders(httpResponse.Headers),
                StatusCode = (long) httpResponse.StatusCode
            };

            var bytes = response.Raw = httpResponse.Content.ReadAsByteArrayAsync().Result;
            try
            {
                object value;
                switch (serialization)
                {
                    case SerializationType.Json:
                    {
                        _serializer.Deserialize(typeof(T), ref bytes, out value);
                        break;
                    }
                    default:
                    {
                        value = bytes;
                        break;
                    }
                }

                response.Payload = (T)value;

                if (httpResponse.IsSuccessStatusCode)
                {
                    response.NetworkSuccess = true;
                }
                else
                {
                    response.NetworkSuccess = false;
                    response.NetworkError = httpResponse.ReasonPhrase;
                }
            }
            catch (Exception exception)
            {
                response.NetworkSuccess = false;
                response.NetworkError = $"Could not deserialize {httpResponse.ReasonPhrase} : {exception.Message}.";
            }

            return response;
        }

        /// <summary>
        /// Gets an equivalent <see cref="HttpMethod"/> for a <see cref="HttpVerb"/>
        /// </summary>
        private HttpMethod MethodFor(HttpVerb verb)
        {
            switch (verb)
            {
                case HttpVerb.Get: return HttpMethod.Get;
                case HttpVerb.Post: return HttpMethod.Post;
                case HttpVerb.Put: return HttpMethod.Put;
                case HttpVerb.Patch: return new HttpMethod("PATCH");
                case HttpVerb.Delete: return HttpMethod.Delete;
                default: return new HttpMethod(verb.ToString().ToUpperInvariant());
            }
        }

        /// <summary>
        /// Converts headers in a Dictionary to a list of CreateAR.Commons.Unity.DataStructures.Tuples.
        /// </summary>
        /// <param name="headers">The headers in a dictionary.</param>
        /// <returns>A list of CreateAR.Commons.Unity.DataStructures.Tuples that represent the headers.</returns>
        private List<DataStructures.Tuple<string, string>> FormatHeaders(HttpResponseHeaders headers)
        {
            var tuples = new List<DataStructures.Tuple<string, string>>();

            if (null != headers)
            {
                foreach (var kvPair in headers)
                {
                    tuples.Add(
                        DataStructures.Tuple.Create(
                            kvPair.Key,
                            kvPair.Value.First()));
                }
            }

            return tuples;
        }
    }
}