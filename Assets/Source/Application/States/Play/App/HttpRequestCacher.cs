using System;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Caches HTTP requests.
    /// </summary>
    public class HttpRequestCacher
    {
        /// <summary>
        /// Determines how the cache operates.
        /// </summary>
        public enum LoadBehavior
        {
            NetworkFirst,
            DiskOnly
        }

        /// <summary>
        /// Manages fils.
        /// </summary>
        private readonly IFileManager _files;

        /// <summary>
        /// Constructor.
        /// </summary>
        public HttpRequestCacher(IFileManager files)
        {
            _files = files;
        }

        /// <summary>
        /// Makes a request using the passed in behavior.
        /// </summary>
        /// <param name="behavior">The behavior.</param>
        /// <param name="key">The key to store the request under in the file system.</param>
        /// <param name="func">A method that executes the http request.</param>
        /// <returns></returns>
        public IAsyncToken<T> Request<T>(
            LoadBehavior behavior,
            string key,
            Func<IAsyncToken<HttpResponse<T>>> func)
        {
            var token = new AsyncToken<T>();
            
            Exception networkException = null;

            Action fromDisk = () =>
            {
                _files
                    .Get<T>(key)
                    .OnSuccess(file => token.Succeed(file.Data))
                    .OnFailure(exception =>
                    {
                        Log.Info(this, "Could not load from disk : {0}.", exception);

                        if (null != networkException)
                        {
                            var aggregate = new AggregateException();
                            aggregate.Exceptions.Add(networkException);
                            aggregate.Exceptions.Add(exception);

                            token.Fail(aggregate);
                        }
                        else
                        {
                            token.Fail(exception);
                        }
                    });
            };

            if (behavior == LoadBehavior.DiskOnly)
            {
                fromDisk();
            }
            else
            {
                func()
                    .OnSuccess(response =>
                    {
                        _files
                            .Set(key, response.Payload)
                            .OnFailure(exception => Log.Warning(this, "Could not write {0}:{1} to disk : {2}.",
                                key,
                                response.Payload.GetType().Name,
                                exception));

                        token.Succeed(response.Payload);
                    })
                    .OnFailure(exception =>
                    {
                        Log.Info(this, "Could not load from network : {0}.", exception);

                        networkException = exception;

                        fromDisk();
                    });
            }

            return token;
        }
    }
}