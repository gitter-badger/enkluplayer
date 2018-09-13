using LightJson;

namespace CreateAR.EnkluPlayer
{
    public class ScriptListEvent
    {
        [JsonName("scripts")]
        public ScriptData[] Scripts;
    }

    public class ScriptAddEvent
    {
        [JsonName("script")]
        public ScriptData Script;
    }

    public class ScriptUpdateEvent
    {
        [JsonName("script")]
        public ScriptData Script;
    }

    public class ScriptRemoveEvent
    {
        [JsonName("id")]
        public string Id;
    }
}