using LightJson;

namespace CreateAR.EnkluPlayer
{
    public class AssetListEvent
    {
        [JsonName("assets")]
        public AssetData[] Assets;
    }

    public class AssetAddEvent
    {
        [JsonName("asset")]
        public AssetData Asset;
    }

    public class AssetUpdateEvent
    {
        [JsonName("asset")]
        public AssetData Asset;
    }

    public class AssetStatsEvent
    {
        [JsonName("assetId")]
        public string Id;

        [JsonName("stats")]
        public AssetStatsData Stats;
    }

    public class AssetDeleteEvent
    {
        [JsonName("assetId")]
        public string Id;
    }
}