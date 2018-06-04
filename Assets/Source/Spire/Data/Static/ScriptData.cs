using LightJson;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Describes type of script.
    /// </summary>
    public enum ScriptType
    {
        Behavior,
        Vine
    }
    
    /// <summary>
    /// Describes a script.
    /// </summary>
    public class ScriptData : StaticData
    {
        /// <summary>
        /// Backing variable for property.
        /// </summary>
        private string[] _tags;

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
        public string TagString;

        /// <summary>
        /// CRC.
        /// </summary>
        [JsonName("crc")]
        public string Crc;

        /// <summary>
        /// Time at which script was created.
        /// </summary>
        [JsonName("createdAt")]
        public string CreatedAt;

        /// <summary>
        /// Time at which script was last updated.
        /// </summary>
        [JsonName("updatedAt")]
        public string UpdatedAt;

        /// <summary>
        /// Id of owning user.
        /// </summary>
        [JsonName("owner")]
        public string Owner;

        /// <summary>
        /// Version.
        /// </summary>
        [JsonName("version")]
        public int Version;

        /// <summary>
        /// Retieves the type of script.
        /// </summary>
        public ScriptType Type
        {
            get
            {
                var tags = Tags;
                for (int i = 0, len = tags.Length; i < len; i++)
                {
                    var tag = tags[i];
                    if (tag.ToLowerInvariant() == "vine")
                    {
                        return ScriptType.Vine;
                    }
                }

                return ScriptType.Behavior;
            }
        }

        /// <summary>
        /// Useful ToString().
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("[ScriptData Id={0}, Type={1}, Uri={2}, Version={3}]",
                Id,
                Type,
                Uri,
                Version);
        }

        /// <summary>
        /// Tags.
        /// </summary>
        public string[] Tags
        {
            get
            {
                if (null == _tags)
                {
                    _tags = (TagString ?? "").Split(',');
                }

                return _tags;
            }
        }
    }
}