using LightJson;

namespace CreateAR.EnkluPlayer
{
    public class MaterialListEvent
    {
        [JsonName("materials")]
        public MaterialData[] Materials;
    }

    public class MaterialAddEvent
    {
        [JsonName("material")]
        public MaterialData Material;
    }

    public class MaterialUpdateEvent
    {
        [JsonName("material")]
        public MaterialData Material;
    }

    public class MaterialRemoveEvent
    {
        [JsonName("id")]
        public string Id;
    }
}