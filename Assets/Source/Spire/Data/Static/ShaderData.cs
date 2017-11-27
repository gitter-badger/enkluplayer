using Newtonsoft.Json;

namespace CreateAR.SpirePlayer
{
    public class ShaderFormPropData
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("uniform")]
        public string Uniform { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }

    public class ShaderFormTextureData
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("uniform")]
        public string Uniform { get; set; }
    }

    public class ShaderFormData
    {
        [JsonProperty("properties")]
        public ShaderFormPropData[] Properties { get; set; }

        [JsonProperty("textures")]
        public ShaderFormTextureData[] Textures { get; set; }
    }

    public class ShaderData : StaticData
    {
        [JsonProperty("tags")]
        public string Tags { get; set; }

        [JsonProperty("form")]
        public ShaderFormData Form { get; set; }
    }
}