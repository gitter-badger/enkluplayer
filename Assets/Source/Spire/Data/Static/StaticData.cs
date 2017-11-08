using System;
using Newtonsoft.Json;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Base class for all Data classes.
    /// </summary>
    [Serializable]
    public class StaticData
    {
        /// <summary>
        /// Unique identifier.
        /// </summary>
        [JsonProperty("id")]
        public string Id;

        /// <summary>
        /// Human readable name. Non-unique.
        /// </summary>
        [JsonProperty("name")]
        public string Name;

        /// <summary>
        /// Optional description.
        /// </summary>
        [JsonProperty("description")]
        public string Description;

        /// <summary>
        /// Useful ToString.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("[{0} Id={1}, Name={2}, Description={3}]",
                GetType().Name,
                Id,
                Name,
                Description);
        }
    }
}