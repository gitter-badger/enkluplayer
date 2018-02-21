﻿using LightJson;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Base class for all scene events.
    /// </summary>
    public abstract class SceneEvent
    {
        /// <summary>
        /// Type of event.
        /// </summary>
        [JsonName("type")]
        public int Type;

        /// <summary>
        /// Source user id.
        /// </summary>
        [JsonName("user")]
        public string User;

        /// <summary>
        /// Id of scene.
        /// </summary>
        [JsonName("sceneId")]
        public string Scene;
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
    }

    /// <summary>
    /// Event for scene deletes.
    /// </summary>
    public class SceneDeleteEvent : SceneEvent
    {
        // 
    }
}