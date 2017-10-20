using Newtonsoft.Json;

namespace CreateAR.SpirePlayer
{
    public class HierarchyListEvent
    {
        [JsonProperty("children")]
        public HierarchyNodeData[] Children;
    }

    public class HierarchySelectEvent
    {
        [JsonProperty("contentId")]
        public string ContentId { get; set; }
    }

    public class HierarchyRemoveEvent
    {
        [JsonProperty("contentId")]
        public string ContentId { get; set; }
    }

    public class HierarchyAddEvent
    {
        [JsonProperty("parent")]
        public string Parent { get; set; }

        [JsonProperty("node")]
        public HierarchyNodeData Node { get; set; }
    }

    public class HierarchyUpdateEvent
    {
        [JsonProperty("node")]
        public HierarchyNodeData Node { get; set; }
    }
}