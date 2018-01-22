using LightJson;

namespace CreateAR.SpirePlayer
{
    public class ShaderListEvent
    {
        [JsonName("shaders")]
        public ShaderData[] Shaders;
    }

    public class ShaderAddEvent
    {
        [JsonName("shader")]
        public ShaderData Shader;
    }

    public class ShaderUpdateEvent
    {
        [JsonName("shader")]
        public ShaderData Shader;
    }

    public class ShaderRemoveEvent
    {
        [JsonName("id")]
        public string Id;
    }
}
