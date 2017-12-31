using LightJson;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Describes a script.
    /// </summary>
    public class ScriptData : StaticData
    {
        /// <summary>
        /// The URI at which to download the script. This is not a complete URI
        /// but used to create a complete URI.
        /// </summary>
        [JsonName("uri")]
        public string Uri;

        /// <summary>
        /// If true, plays on its own.
        /// </summary>
        [JsonName("autoplay")]
        public bool AutoPlay;

        /// <summary>
        /// Tags associated with this script.
        /// </summary>
        [JsonName("tags")]
        public string[] Tags;

        /// <summary>
        /// Useful ToString().
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("[ScriptData Id={0}, Uri={1}]",
                Id,
                Uri);
        }
    }
}