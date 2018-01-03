using System;
using LightJson;

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
        [JsonName("id")]
        public string AssetDataId;
        
        /// <summary>
        /// Describes how this asset should be pooled.
        /// </summary>
        [JsonName("pooling")]
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