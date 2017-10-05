using System;
using System.Collections.Generic;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Data that describes a scene.
    /// </summary>
    [Serializable]
    public class SceneData : StaticData
    {
        /// <summary>
        /// Script GUIDs withing this scene.
        /// </summary>
        public List<string> Scripts = new List<string>();

        /// <summary>
        /// Content GUIDs within this scene.
        /// </summary>
        public List<string> Content = new List<string>();
    }
}