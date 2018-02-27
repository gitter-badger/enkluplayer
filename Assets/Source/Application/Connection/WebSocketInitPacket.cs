using LightJson;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Init packet we receive from Sails.
    /// </summary>
    public class WebSocketInitPacket
    {
        [JsonName("sid")]
        public string Sid;

        [JsonName("upgrades")]
        public string[] Upgrades;

        [JsonName("pingInterval")]
        public int PingInterval;

        [JsonName("pingTimeout")]
        public int PingTimeout;
    }
}