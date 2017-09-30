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
        /// Readable name.
        /// </summary>
        public string Name;
        
        /// <summary>
        /// Content within this scene.
        /// </summary>
        public List<string> Content = new List<string>();
    }
}