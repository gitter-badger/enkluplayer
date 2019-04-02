using System;
using CreateAR.Commons.Unity.Async;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Keeps a resolution for an <c>AsyncToken</c> separate from the token.
    /// This is particularly useful for passing resolution between threads.
    /// </summary>
    /// <typeparam name="T">The type of resolution supported.</typeparam>
    public class AsyncResolution<T>
    {
        /// <summary>
        /// The success value.
        /// </summary>
        public T Value { get; private set; }

        /// <summary>
        /// The error value.
        /// </summary>
        public Exception Exception { get; private set; }

        /// <summary>
        /// True iff resolved.
        /// </summary>
        private bool _isResolved;

        /// <summary>
        /// Resolves successfully.
        /// </summary>
        /// <param name="value"></param>
        public void Resolve(T value)
        {
            if (_isResolved)
            {
                throw new Exception("Resolution is already resolved!");
            }

            Value = value;
            _isResolved = true;
        }

        /// <summary>
        /// Resolves with an exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public void Resolve(Exception exception)
        {
            if (_isResolved)
            {
                throw new Exception("Resolution is already resolved!");
            }

            Exception = exception;
            _isResolved = true;
        }

        /// <summary>
        /// Attempts to apply the resolution to a token. Returns true iff the
        /// resolution was already resolved.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        public bool Apply(AsyncToken<T> token)
        {
            if (!_isResolved)
            {
                return false;
            }

            if (null == Exception)
            {
                token.Fail(Exception);
            }
            else
            {
                token.Succeed(Value);
            }

            return true;
        }
    }
}