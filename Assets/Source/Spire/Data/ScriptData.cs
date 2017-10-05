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
        /// If true, plays on its own.
        /// </summary>
        public bool AutoPlay;

        /// <summary>
        /// Tags associated with this script.
        /// </summary>
        public string[] Tags;
    }
}