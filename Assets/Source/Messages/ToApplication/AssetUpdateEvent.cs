using Newtonsoft.Json;

namespace CreateAR.SpirePlayer
{
    public class AssetUpdateEvent
    {
        [JsonProperty("assets")]
        public AssetData[] Assets;
    }
}