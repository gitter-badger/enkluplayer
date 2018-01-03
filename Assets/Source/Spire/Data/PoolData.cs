using System;
using LightJson;

namespace CreateAR.SpirePlayer
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
        [JsonName("enabled")]
        public bool Enabled;

        /// <summary>
        /// Pool priority.
        /// </summary>
        [JsonName("priority")]
        public int Priority;

        /// <summary>
        /// How big the pool should start.
        /// </summary>
        [JsonName("startSize")]
        public int StartSize = 4;

        /// <summary>
        /// How mucht he pool should grow by.
        /// </summary>
        [JsonName("growSize")]
        public int GrowSize = 2;

        /// <summary>
        /// The maximum size of the pool.
        /// </summary>
        [JsonName("maxSize")]
        public int MaxSize = 0;
    }
}