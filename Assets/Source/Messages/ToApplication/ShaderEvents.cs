using Newtonsoft.Json;

namespace CreateAR.SpirePlayer
{
    public class ShaderListEvent
    {
        [JsonProperty("Shaders")]
        public ShaderData[] Shaders;
    }

    public class ShaderAddEvent
    {
        [JsonProperty("Shader")]
        public ShaderData Shader;
    }

    public class ShaderUpdateEvent
    {
        [JsonProperty("Shader")]
        public ShaderData Shader;
    }

    public class ShaderRemoveEvent
    {
        [JsonProperty("id")]
        public string Id;
    }
}
