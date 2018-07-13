using System;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;

namespace CreateAR.SpirePlayer
{
    public static class HttpRetrier
    {
        public static IAsyncToken<HttpResponse<T>> Try<T>(
            Func<IAsyncToken<HttpResponse<T>>> makeRequest,
            int numTries = 3)
        {
            throw new NotImplementedException();
        }
    }
}
