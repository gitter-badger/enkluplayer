using System;
using System.Collections.Generic;

namespace CreateAR.Commons.Unity.Async
{
    /// <summary>
    /// Helpful methods for <c>IAsyncToken</c>.
    /// </summary>
    public static class Async
    {
        /// <summary>
        /// Creates a single token from a collection of tokens.
        /// 
        /// A failure from any one of the tokens will result in a failure of
        /// the returned token. If only a single token fails, only that exception
        /// is returned. If multiple tokens fail, an <c>AggregateException</c>
        /// is returned.
        /// 
        /// If no tokens are passed in, the returned token is a Success.
        /// 
        /// TODO: TESTS.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tokens"></param>
        /// <returns></returns>
        public static IAsyncToken<T[]> All<T>(params IAsyncToken<T>[] tokens)
        {
            var len = tokens.Length;
            if (0 == len)
            {
                return new AsyncToken<T[]>(new T[0]);
            }

            var returnToken = new AsyncToken<T[]>();
            var values = new T[len];
            var exceptions = new List<Exception>();
            var numReturned = 0;

            for (var i = 0; i < len; i++)
            {
                var token = tokens[i];
                var index = i;
                token
                    .OnSuccess(value => values[index] = value)
                    .OnFailure(exception => exceptions.Add(exception))
                    .OnFinally(_ =>
                    {
                        if (++numReturned == len)
                        {
                            if (exceptions.Count > 1)
                            {
                                var aggregate = new AggregateException();
                                aggregate.Exceptions.AddRange(exceptions);

                                returnToken.Fail(aggregate);
                            }
                            else if (exceptions.Count == 1)
                            {
                                returnToken.Fail(exceptions[0]);
                            }
                            else
                            {
                                returnToken.Succeed(values);
                            }
                        }
                    });
            }

            return returnToken;
        }

        /// <summary>
        /// Creates a single token from a collection of tokens.
        /// 
        /// A failure from any one of the tokens will result in a failure of
        /// the returned token. If only a single token fails, only that exception
        /// is returned. If multiple tokens fail, an <c>AggregateException</c>
        /// is returned.
        /// 
        /// If no tokens are passed in, the returned token is a Success.
        /// 
        /// TODO: TESTS.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tokens"></param>
        /// <returns></returns>
        public static IMutableAsyncToken<T[]> All<T>(params IMutableAsyncToken<T>[] tokens)
        {
            var len = tokens.Length;
            if (0 == len)
            {
                return new MutableAsyncToken<T[]>(new T[0]);
            }

            var returnToken = new MutableAsyncToken<T[]>();
            var values = new T[len];
            var exceptions = new List<Exception>();
            var numReturned = 0;

            for (var i = 0; i < len; i++)
            {
                var token = tokens[i];
                var index = i;
                token
                    .OnSuccess(value => values[index] = value)
                    .OnFailure(exception => exceptions.Add(exception))
                    .OnFinally(_ =>
                    {
                        // TODO: THIS IS INACCURATE! A single token could be resolved LEN times
                        // TODO: and this would still be satisified. Hmm...
                        if (++numReturned == len)
                        {
                            if (exceptions.Count > 1)
                            {
                                var aggregate = new AggregateException();
                                aggregate.Exceptions.AddRange(exceptions);

                                returnToken.Fail(aggregate);
                            }
                            else if (exceptions.Count == 1)
                            {
                                returnToken.Fail(exceptions[0]);
                            }
                            else
                            {
                                returnToken.Succeed(values);
                            }
                        }
                    });
            }

            return returnToken;
        }
        
        /// <summary>
        /// Maps a token to another token type.
        /// </summary>
        /// <typeparam name="T">Starting generic parameter.</typeparam>
        /// <typeparam name="R">Result generic parameter/</typeparam>
        /// <param name="token">Starting token.</param>
        /// <param name="map">Function to map between types.</param>
        /// <returns></returns>
        public static IAsyncToken<R> Map<T, R>(
            this IAsyncToken<T> token,
            Func<T, R> map)
        {
            var output = new AsyncToken<R>();

            token
                .OnSuccess(value => output.Succeed(map(value)))
                .OnFailure(output.Fail);

            return output;
        }

        /// <summary>
        /// Maps a token to another token type.
        /// </summary>
        /// <typeparam name="T">Starting generic parameter.</typeparam>
        /// <typeparam name="R">Result generic parameter/</typeparam>
        /// <param name="token">Starting token.</param>
        /// <param name="map">Function to map between types.</param>
        /// <returns></returns>
        public static IMutableAsyncToken<R> Map<T, R>(
            IMutableAsyncToken<T> token,
            Func<T, R> map)
        {
            var output = new MutableAsyncToken<R>();

            token
                .OnSuccess(value => output.Succeed(map(value)))
                .OnFailure(output.Fail);

            return output;
        }
    }
}