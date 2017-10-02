using System;
using System.Collections.Generic;

namespace CreateAR.Spire
{
    /// <summary>
    /// Data that describes a scene.
    /// </summary>
    [Serializable]
    public class SceneData : StaticData
    {
        /// <summary>
        /// Scene scripts.
        /// </summary>
        public List<ScriptData> Scripts = new List<ScriptData>();

        /// <summary>
        /// Content GUIDs within this scene.
        /// </summary>
        public List<string> Content = new List<string>();
    }
}