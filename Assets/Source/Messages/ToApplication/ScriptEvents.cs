using LightJson;

namespace CreateAR.SpirePlayer
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
        [JsonName("Script")]
        public ScriptData Script;
    }

    public class ScriptRemoveEvent
    {
        [JsonName("id")]
        public string Id;
    }
}