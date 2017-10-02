namespace CreateAR.Spire
{
    /// <summary>
    /// Describes a script.
    /// </summary>
    public class ScriptInfo
    {
        /// <summary>
        /// Identifier unique to this script.
        /// </summary>
        public string Guid;
        
        /// <summary>
        /// The URI at which to download the script. This is not a complete URI
        /// but used to create a complete URI.
        /// </summary>
        public string Uri;
        
        /// <summary>
        /// Version of the script.
        /// </summary>
        public int Version;

        /// <summary>
        /// Crc for checking download validity.
        /// </summary>
        public string Crc;

        /// <summary>
        /// Tags associated with this script.
        /// </summary>
        public string[] Tags;

        /// <summary>
        /// Useful ToString.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("[ScriptInfo Guid={0}, Uri={1}]",
                Guid,
                Uri);
        }
    }
}