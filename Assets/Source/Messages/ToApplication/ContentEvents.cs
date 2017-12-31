using LightJson;

namespace CreateAR.SpirePlayer
{
    public class ContentListEvent
    {
        [JsonName("content")]
        public ContentData[] Content;
    }

    public class ContentAddEvent
    {
        [JsonName("content")]
        public ContentData Content;
    }

    public class ContentUpdateEvent
    {
        [JsonName("content")]
        public ContentData Content;
    }

    public class ContentRemoveEvent
    {
        [JsonName("id")]
        public string Id;
    }
}