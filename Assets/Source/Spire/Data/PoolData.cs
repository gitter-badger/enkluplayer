using System;

namespace CreateAR.Spire
{
    /// <summary>
    /// Data describing how to pool.
    /// </summary>
    [Serializable]
    public class PoolData
    {
        /// <summary>
        /// True iff pooling is enabled.
        /// </summary>
        public bool Enabled;

        /// <summary>
        /// Pool priority.
        /// </summary>
        public int Priority;

        /// <summary>
        /// How big the pool should start.
        /// </summary>
        public int StartSize = 4;

        /// <summary>
        /// How mucht he pool should grow by.
        /// </summary>
        public int GrowSize = 2;

        /// <summary>
        /// The maximum size of the pool.
        /// </summary>
        public int MaxSize = 2;
    }
}