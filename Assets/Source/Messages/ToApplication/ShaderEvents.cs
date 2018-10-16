using Newtonsoft.Json;

namespace CreateAR.EnkluPlayer
{
    public class ShaderListEvent
    {
        [JsonProperty("shaders")]
        public ShaderData[] Shaders;
    }

    public class ShaderAddEvent
    {
        [JsonProperty("shader")]
        public ShaderData Shader;
    }

    public class ShaderUpdateEvent
    {
        [JsonProperty("shader")]
        public ShaderData Shader;
    }

    public class ShaderRemoveEvent
    {
        [JsonProperty("id")]
        public string Id;
    }
}
