using System;
using Newtonsoft.Json;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Data describing how to use an Asset.
    /// </summary>
    [Serializable]
    public class AssetReference
    {
        /// <summary>
        /// Unique id of the AssetData.
        /// </summary>
        [JsonProperty("id")]
        public string AssetDataId;
        
        /// <summary>
        /// Describes how this asset should be pooled.
        /// </summary>
        [JsonProperty("pooling")]
        public PoolData Pooling;

        /// <summary>
        /// Useful ToString.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format(
                "[AssetReference AssetDataId={0}]",
                AssetDataId);
        }
    }
}