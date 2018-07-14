using System.Text;
using LightJson;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Base class for all scene events.
    /// </summary>
    public abstract class SceneEvent
    {
        /// <summary>
        /// Txn id.
        /// </summary>
        [JsonName("id")]
        public long Id;

        /// <summary>
        /// Type of event.
        /// </summary>
        [JsonName("type")]
        public int Type;

        /// <summary>
        /// Id of scene.
        /// </summary>
        [JsonName("sceneId")]
        public string Scene;

        /// <summary>
        /// Id of user.
        /// </summary>
        [JsonName("user")]
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
        [JsonName("actions")]
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