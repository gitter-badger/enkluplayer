using Newtonsoft.Json;

namespace CreateAR.EnkluPlayer
{
    public class ScriptListEvent
    {
        [JsonProperty("scripts")]
        public ScriptData[] Scripts;
    }

    public class ScriptAddEvent
    {
        [JsonProperty("script")]
        public ScriptData Script;
    }

    public class ScriptUpdateEvent
    {
        [JsonProperty("script")]
        public ScriptData Script;
    }

    public class ScriptRemoveEvent
    {
        [JsonProperty("id")]
        public string Id;
    }
}