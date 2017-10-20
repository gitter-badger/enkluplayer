using Newtonsoft.Json;

namespace CreateAR.SpirePlayer
{
    public class AssetListEvent
    {
        [JsonProperty("assets")]
        public AssetData[] Assets;
    }

    public class AssetAddEvent
    {
        [JsonProperty("asset")]
        public AssetData Asset;
    }

    public class AssetRemoveEvent
    {
        [JsonProperty("id")]
        public string Id;
    }

    public class AssetUpdateEvent
    {
        [JsonProperty("asset")]
        public AssetData Asset;
    }
}