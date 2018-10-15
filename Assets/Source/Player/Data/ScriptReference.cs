using System;
using Newtonsoft.Json;

namespace CreateAR.EnkluPlayer
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
        [JsonProperty("id")]
        public string ScriptDataId;

        /// <summary>
        /// If true, starts the script immediately after loading.
        /// </summary>
        [JsonProperty("playOnAwake")]
        public bool PlayOnAwake;
    }
}