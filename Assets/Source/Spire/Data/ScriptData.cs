using Newtonsoft.Json;

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
        [JsonProperty("uri")]
        public string Uri;

        /// <summary>
        /// If true, plays on its own.
        /// </summary>
        [JsonProperty("autoplay")]
        public bool AutoPlay;

        /// <summary>
        /// Tags associated with this script.
        /// </summary>
        [JsonProperty("tags")]
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