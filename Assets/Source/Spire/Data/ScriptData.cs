using System;

namespace CreateAR.Spire
{
    /// <summary>
    /// Data about a script.
    /// </summary>
    [Serializable]
    public class ScriptData
    {
        /// <summary>
        /// Unique id of the script to load.
        /// </summary>
        public string ScriptId;

        /// <summary>
        /// Useful ToString.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("[ScriptData ScriptId={0}]", ScriptId);
        }
    }
}