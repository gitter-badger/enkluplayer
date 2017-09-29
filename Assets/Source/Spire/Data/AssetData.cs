using System;

namespace CreateAR.Spire
{
    /// <summary>
    /// Data describing how to use an Asset.
    /// </summary>
    [Serializable]
    public class AssetData
    {
        /// <summary>
        /// Unique id of the AssetInfo.
        /// </summary>
        public string AssetInfoId;
        
        /// <summary>
        /// Describes how this asset should be pooled.
        /// </summary>
        public PoolData Pooling;

        /// <summary>
        /// Useful ToString.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format(
                "[AssetData AssetInfoId={0}]",
                AssetInfoId);
        }
    }
}