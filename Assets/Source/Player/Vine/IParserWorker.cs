using System;

namespace CreateAR.EnkluPlayer.Vine
{
    /// <summary>
    /// Describes an object that can parse.
    /// </summary>
    public interface IParserWorker
    {
        /// <summary>
        /// Starts worker.
        /// </summary>
        void Start();

        /// <summary>
        /// Queues an action.
        /// </summary>
        /// <param name="action">The action to run.</param>
        /// <param name="callback">The callback.</param>
        void Enqueue<T>(Func<T> action, Action<T> callback);
    }
}