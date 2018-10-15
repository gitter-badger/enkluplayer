using Newtonsoft.Json;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Init packet we receive from Sails.
    /// </summary>
    public class WebSocketInitPacket
    {
        [JsonProperty("sid")]
        public string Sid { get; set; }

        [JsonProperty("upgrades")]
        public string[] Upgrades { get; set; }

        [JsonProperty("pingInterval")]
        public int PingInterval { get; set; }

        [JsonProperty("pingTimeout")]
        public int PingTimeout { get; set; }
    }
}