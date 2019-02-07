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
            var tokenReturned = new bool[len];
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
                        if (!tokenReturned[index])
                        {
                            tokenReturned[index] = true;
                            numReturned++;
                        }
                        
                        if (numReturned == len)
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

        public static IAsyncToken<T[]> All<T>(IMutableAsyncToken<T>[] mutableTokens, IAsyncToken<T>[] asyncTokens)
        {
            return All(asyncTokens, mutableTokens);
        }

        public static IAsyncToken<T[]> All<T>(IAsyncToken<T>[] asyncTokens, IMutableAsyncToken<T>[] mutableTokens)
        {
            var singleLen = asyncTokens.Length;
            var mutableLen = mutableTokens.Length;
            var len = singleLen + mutableLen;
            if (0 == len)
            {
                return new AsyncToken<T[]>(new T[0]);
            }

            var rtnToken = new AsyncToken<T[]>();
            var rtnValues = new List<T>(len);
            var singleReturned = false;
            var mutableReturned = false;

            Exception singleException = null;
            Exception mutableException = null;

            Action resolveToken = () =>
            {
                if (singleException != null && mutableException != null)
                {
                    var aggException = new AggregateException();
                    aggException.Exceptions.Add(singleException);
                    aggException.Exceptions.Add(mutableException);
                    rtnToken.Fail(aggException);
                    return;
                }

                if (singleException != null)
                {
                    rtnToken.Fail(singleException);
                    return;
                }

                if (mutableException != null)
                {
                    rtnToken.Fail(mutableException);
                    return;
                }

                rtnToken.Succeed(rtnValues.ToArray());
            };

            All(asyncTokens)
                .OnSuccess(val => rtnValues.AddRange(val))
                .OnFailure(exception => singleException = exception)
                .OnFinally(_ =>
                {
                    singleReturned = true;
                    if (mutableReturned)
                    {
                        resolveToken();
                    }
                });
            All(mutableTokens)
                .OnSuccess(val => rtnValues.AddRange(val))
                .OnFailure(exception => mutableException = exception)
                .OnFinally(_ =>
                {
                    mutableReturned = true;
                    if (singleReturned)
                    {
                        resolveToken();
                    }
                });
            
            return rtnToken;
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
            IAsyncToken<T> token,
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

        /// <summary>
        /// Makes an immutable token out of a mutable token.
        /// </summary>
        /// <typeparam name="T">Type parameter.</typeparam>
        /// <param name="mutableToken">The mutable token to convert.</param>
        /// <returns></returns>
        public static IAsyncToken<T> ToImmutable<T>(IMutableAsyncToken<T> mutableToken)
        {
            var token = new AsyncToken<T>();

            mutableToken
                .OnSuccess(token.Succeed)
                .OnFailure(token.Fail);

            return token;
        }
    }
}