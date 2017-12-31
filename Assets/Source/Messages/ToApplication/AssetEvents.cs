using LightJson;

namespace CreateAR.SpirePlayer
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

    public class AssetRemoveEvent
    {
        [JsonName("id")]
        public string Id;
    }

    public class AssetUpdateEvent
    {
        [JsonName("asset")]
        public AssetData Asset;
    }
}