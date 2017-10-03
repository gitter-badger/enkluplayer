using System;

namespace CreateAR.Spire
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
        public string AssetDataId;
        
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
                "[AssetData AssetDataId={0}]",
                AssetDataId);
        }
    }
}