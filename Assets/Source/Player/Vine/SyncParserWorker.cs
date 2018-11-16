using System;

namespace CreateAR.EnkluPlayer.Vine
{
    /// <summary>
    /// Parser worker that runs on main thread.
    /// </summary>
    public class SyncParserWorker : IParserWorker
    {
        /// <inheritdoc />
        public void Start()
        {
            //
        }

        /// <inheritdoc />
        public void Enqueue<T>(Func<T> action, Action<T> callback)
        {
            callback(action());
        }
    }
}