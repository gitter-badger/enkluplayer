using Newtonsoft.Json;

namespace CreateAR.SpirePlayer
{
    public class MaterialData
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("shaderId")]
        public string ShaderId { get; set; }
    }
}