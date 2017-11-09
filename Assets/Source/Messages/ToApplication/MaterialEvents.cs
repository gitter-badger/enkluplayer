using Newtonsoft.Json;

namespace CreateAR.SpirePlayer
{
    public class MaterialListEvent
    {
        [JsonProperty("materials")]
        public MaterialData[] Materials;
    }

    public class MaterialAddEvent
    {
        [JsonProperty("material")]
        public MaterialData Material;
    }

    public class MaterialUpdateEvent
    {
        [JsonProperty("material")]
        public MaterialData Material;
    }

    public class MaterialRemoveEvent
    {
        [JsonProperty("id")]
        public string Id;
    }
}