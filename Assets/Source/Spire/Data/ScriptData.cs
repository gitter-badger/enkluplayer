namespace CreateAR.Spire
{
    /// <summary>
    /// Describes a script.
    /// </summary>
    public class ScriptData : StaticData
    {
        /// <summary>
        /// Reference to asset.
        /// </summary>
        public AssetReference Asset;

        /// <summary>
        /// Tags associated with this script.
        /// </summary>
        public string[] Tags;
    }
}