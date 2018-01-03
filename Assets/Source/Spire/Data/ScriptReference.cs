using System;
using LightJson;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Data describing how to use a Script.
    /// </summary>
    [Serializable]
    public class ScriptReference
    {
        /// <summary>
        /// Unique id of the script to play.
        /// </summary>
        [JsonName("id")]
        public string ScriptDataId;

        /// <summary>
        /// If true, starts the script immediately after loading.
        /// </summary>
        [JsonName("playOnAwake")]
        public bool PlayOnAwake;
    }
}