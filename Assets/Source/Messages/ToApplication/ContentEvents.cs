using Newtonsoft.Json;

namespace CreateAR.SpirePlayer
{
    public class ContentListEvent
    {
        [JsonProperty("content")]
        public ContentData[] Content;
    }

    public class ContentAddEvent
    {
        [JsonProperty("content")]
        public ContentData Content;
    }

    public class ContentUpdateEvent
    {
        [JsonProperty("content")]
        public ContentData Content;
    }

    public class ContentRemoveEvent
    {
        [JsonProperty("id")]
        public string Id;
    }
}