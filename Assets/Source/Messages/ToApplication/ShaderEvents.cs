using LightJson;

namespace CreateAR.SpirePlayer
{
    public class ShaderListEvent
    {
        [JsonName("Shaders")]
        public ShaderData[] Shaders;
    }

    public class ShaderAddEvent
    {
        [JsonName("Shader")]
        public ShaderData Shader;
    }

    public class ShaderUpdateEvent
    {
        [JsonName("Shader")]
        public ShaderData Shader;
    }

    public class ShaderRemoveEvent
    {
        [JsonName("id")]
        public string Id;
    }
}
