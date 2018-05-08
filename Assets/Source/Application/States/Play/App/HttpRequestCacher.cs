using System;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;

namespace CreateAR.SpirePlayer
{
    public class HttpRequestCacher
    {
        public enum LoadBehavior
        {
            NetworkFirst,
            DiskOnly
        }

        private readonly IFileManager _files;

        public HttpRequestCacher(IFileManager files)
        {
            _files = files;
        }

        public IAsyncToken<T> Request<T>(
            LoadBehavior behavior,
            string key,
            Func<IAsyncToken<HttpResponse<T>>> func)
        {
            var token = new AsyncToken<T>();

            Action fromDisk = () =>
            {
                _files
                    .Get<T>(key)
                    .OnSuccess(file => token.Succeed(file.Data))
                    .OnFailure(exception =>
                    {
                        Log.Info(this, "Could not load from disk : {0}.", exception);

                        token.Fail(exception);
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
                            .OnFailure(exception => Log.Error(this, "Could not write {0}:{1} to disk : {2}.",
                                key,
                                response.Payload.GetType().Name,
                                exception));

                        token.Succeed(response.Payload);
                    })
                    .OnFailure(exception =>
                    {
                        Log.Info(this, "Could not load from network : {0}.", exception);

                        fromDisk();
                    });
            }

            return token;
        }
    }
}