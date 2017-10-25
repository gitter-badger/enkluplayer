using Newtonsoft.Json;

namespace CreateAR.SpirePlayer
{
    public class ScriptRecord
    {
        [JsonProperty("data")]
        public ScriptData Data;

        [JsonProperty("asset")]
        public AssetData Asset;
    }

    public class ScriptListEvent
    {
        [JsonProperty("scripts")]
        public ScriptRecord[] Scripts;
    }

    public class ScriptAddEvent
    {
        [JsonProperty("script")]
        public ScriptRecord Script;
    }

    public class ScriptUpdateEvent
    {
        [JsonProperty("Script")]
        public ScriptRecord Script;
    }

    public class ScriptRemoveEvent
    {
        [JsonProperty("id")]
        public string Id;
    }
}