using LightJson;

namespace CreateAR.SpirePlayer
{
    public abstract class SceneEvent
    {
        [JsonName("type")]
        public int Type;

        [JsonName("user")]
        public string User;

        [JsonName("sceneId")]
        public string Scene;
    }

    public class SceneCreateEvent : SceneEvent
    {
        
    }

    public class SceneUpdateEvent : SceneEvent
    {
        [JsonName("actions")]
        public ElementActionData[] Actions;
    }

    public class SceneDeleteEvent : SceneEvent
    {

    }
}
