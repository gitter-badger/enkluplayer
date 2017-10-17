using Newtonsoft.Json;

namespace CreateAR.SpirePlayer
{
    public class SelectContentEvent
    {
        [JsonProperty("contentId")]
        public string ContentId { get; set; }
    }

    public class HierarchyEvent
    {
        [JsonProperty("root")]
        public HierarchyNodeData Root;
    }
}