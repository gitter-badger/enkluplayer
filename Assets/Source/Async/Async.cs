using System;
using System.Collections.Generic;

namespace CreateAR.Commons.Unity.Async
{
    public static class Async
    {
        public static IAsyncToken<T[]> All<T>(IAsyncToken<T>[] tokens)
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
    }
}