using Newtonsoft.Json;

namespace CreateAR.SpirePlayer
{
    public class HierarchyNodeData
    {
        [JsonProperty("id")]
        public string Id;

        [JsonProperty("contentId")]
        public string ContentId;

        [JsonProperty("children")]
        public HierarchyNodeData[] Children = new HierarchyNodeData[0];
    }
}