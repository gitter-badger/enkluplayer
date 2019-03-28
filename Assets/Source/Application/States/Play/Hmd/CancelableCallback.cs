using System;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// A callback that can be canceled.
    /// </summary>
    public class CancelableCallback : ICancelable
    {
        /// <summary>
        /// True if the callback has been canceled.
        /// </summary>
        private bool _isCanceled;

        /// <summary>
        /// The callback in question.
        /// </summary>
        private readonly Action _callback;

        /// <summary>
        /// Constructor.
        /// </summary>
        public CancelableCallback(Action callback)
        {
            _callback = callback;
        }

        /// <inheritdoc />
        public void Cancel()
        {
            _isCanceled = true;
        }

        /// <summary>
        /// Tries to invoke the callback.
        /// </summary>
        public void Invoke()
        {
            if (_isCanceled)
            {
                return;
            }

            _callback();
        }
    }
}