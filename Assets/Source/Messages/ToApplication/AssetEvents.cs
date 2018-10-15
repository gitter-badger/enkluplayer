using Newtonsoft.Json;

namespace CreateAR.EnkluPlayer
{
    public class AssetListEvent
    {
        [JsonProperty("assets")]
        public AssetData[] Assets { get; set; }
    }

    public class AssetAddEvent
    {
        [JsonProperty("asset")]
        public AssetData Asset { get; set; }
    }

    public class AssetUpdateEvent
    {
        [JsonProperty("asset")]
        public AssetData Asset { get; set; }
    }

    public class AssetStatsEvent
    {
        [JsonProperty("assetId")]
        public string Id { get; set; }

        [JsonProperty("stats")]
        public AssetStatsData Stats { get; set; }
    }

    public class AssetDeleteEvent
    {
        [JsonProperty("assetId")]
        public string Id { get; set; }
    }
}