using System.Text;
using Newtonsoft.Json;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Base class for all scene events.
    /// </summary>
    public abstract class SceneEvent
    {
        /// <summary>
        /// Txn id.
        /// </summary>
        [JsonProperty("id")]
        public long Id;

        /// <summary>
        /// Type of event.
        /// </summary>
        [JsonProperty("type")]
        public int Type;

        /// <summary>
        /// Id of scene.
        /// </summary>
        [JsonProperty("sceneId")]
        public string Scene;

        /// <summary>
        /// Id of user.
        /// </summary>
        [JsonProperty("user")]
        public string User;
    }

    /// <summary>
    /// Event for scene creation.
    /// </summary>
    public class SceneCreateEvent : SceneEvent
    {
        // 
    }

    /// <summary>
    /// Event for scene update.
    /// </summary>
    public class SceneUpdateEvent : SceneEvent
    {
        /// <summary>
        /// All actions.
        /// </summary>
        [JsonProperty("actions")]
        public ElementActionData[] Actions;

        /// <summary>
        /// Useful ToString.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var builder = new StringBuilder();
            foreach (var action in Actions)
            {
                builder.AppendFormat("\t{0}\n", action);
            }

            return string.Format(
                "[SceneUpdateEvent Type={0}, Actions={1}]",
                Type,
                builder);
        }
    }

    /// <summary>
    /// Event for scene deletes.
    /// </summary>
    public class SceneDeleteEvent : SceneEvent
    {
        // 
    }
}