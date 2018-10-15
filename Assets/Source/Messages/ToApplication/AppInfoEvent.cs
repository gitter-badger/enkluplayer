using Newtonsoft.Json;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Event with application info inside.
    /// </summary>
    public class AppInfoEvent
    {
        /// <summary>
        /// Application id.
        /// </summary>
        [JsonProperty("appId")]
        public string AppId { get; set; }
        
        /// <summary>
        /// Useful ToString.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format(
                "[AppInfoEvent AppId={0}]",
                AppId);
        }
    }
}