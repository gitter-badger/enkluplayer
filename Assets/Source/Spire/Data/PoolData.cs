using System;
using Newtonsoft.Json;

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
        [JsonProperty("enabled")]
        public bool Enabled;

        /// <summary>
        /// Pool priority.
        /// </summary>
        [JsonProperty("priority")]
        public int Priority;

        /// <summary>
        /// How big the pool should start.
        /// </summary>
        [JsonProperty("startSize")]
        public int StartSize = 4;

        /// <summary>
        /// How mucht he pool should grow by.
        /// </summary>
        [JsonProperty("growSize")]
        public int GrowSize = 2;

        /// <summary>
        /// The maximum size of the pool.
        /// </summary>
        [JsonProperty("maxSize")]
        public int MaxSize = 0;
    }
}