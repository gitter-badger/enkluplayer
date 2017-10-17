using Newtonsoft.Json;

namespace CreateAR.SpirePlayer
{
    public class ContentUpdateEvent
    {
        [JsonProperty("content")]
        public ContentData[] Content;
    }
}