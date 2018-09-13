using CreateAR.EnkluPlayer.IUX;
using LightJson;

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
        [JsonName("appId")]
        public string AppId;
        
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