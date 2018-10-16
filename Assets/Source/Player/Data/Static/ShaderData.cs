using Newtonsoft.Json;

namespace CreateAR.EnkluPlayer
{
    public class ShaderFormPropData
    {
        [JsonProperty("name")]
        public string Name;

        [JsonProperty("uniform")]
        public string Uniform;

        [JsonProperty("type")]
        public string Type;
    }

    public class ShaderFormTextureData
    {
        [JsonProperty("name")]
        public string Name;

        [JsonProperty("uniform")]
        public string Uniform;
    }

    public class ShaderFormData
    {
        [JsonProperty("properties")]
        public ShaderFormPropData[] Properties;

        [JsonProperty("textures")]
        public ShaderFormTextureData[] Textures;
    }

    public class ShaderData : StaticData
    {
        [JsonProperty("tags")]
        public string Tags;

        [JsonProperty("form")]
        public ShaderFormData Form;
    }
}