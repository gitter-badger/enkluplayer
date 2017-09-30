using System;
using System.Collections.Generic;

namespace CreateAR.Spire
{
    [Serializable]
    public class AppData : StaticData
    {
        /// <summary>
        /// Name.
        /// </summary>
        public string Name;
        
        /// <summary>
        /// Scenes.
        /// </summary>
        public List<string> Scenes = new List<string>();
    }
}