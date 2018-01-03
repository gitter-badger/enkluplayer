using LightJson;

namespace CreateAR.SpirePlayer
{
    public class ShaderFormPropData
    {
        [JsonName("name")]
        public string Name;

        [JsonName("uniform")]
        public string Uniform;

        [JsonName("type")]
        public string Type;
    }

    public class ShaderFormTextureData
    {
        [JsonName("name")]
        public string Name;

        [JsonName("uniform")]
        public string Uniform;
    }

    public class ShaderFormData
    {
        [JsonName("properties")]
        public ShaderFormPropData[] Properties;

        [JsonName("textures")]
        public ShaderFormTextureData[] Textures;
    }

    public class ShaderData : StaticData
    {
        [JsonName("tags")]
        public string Tags;

        [JsonName("form")]
        public ShaderFormData Form;
    }
}