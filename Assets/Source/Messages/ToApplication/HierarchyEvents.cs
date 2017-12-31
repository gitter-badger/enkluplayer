using LightJson;

namespace CreateAR.SpirePlayer
{
    public class HierarchyListEvent
    {
        [JsonName("children")]
        public HierarchyNodeData[] Children;
    }

    public class HierarchySelectEvent
    {
        [JsonName("contentId")]
        public string ContentId;
    }

    public class HierarchyRemoveEvent
    {
        [JsonName("contentId")]
        public string ContentId;
    }

    public class HierarchyAddEvent
    {
        [JsonName("parent")]
        public string Parent;

        [JsonName("node")]
        public HierarchyNodeData Node;
    }

    public class HierarchyUpdateEvent
    {
        [JsonName("node")]
        public HierarchyNodeData Node;
    }
}